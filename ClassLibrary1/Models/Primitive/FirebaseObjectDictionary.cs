﻿using ObservableHelpers.Observables;
using RestfulFirebase.Database.Models.Primitive;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Models.Primitive
{
    public class FirebaseObjectDictionary : ObservableDictionary<string, FirebaseObject>, IRealtimeModel
    {
        #region Properties

        public RealtimeWire Wire { get; private set; }

        #endregion

        #region Methods

        protected override (string key, FirebaseObject value) ValueFactory(string key, FirebaseObject value)
        {
            value.PropertyChanged += (s, e) =>
            {
                if (value.IsNull())
                {
                    if (this.ContainsKey(key))
                    {
                        Remove(key);
                        Wire?.InvokeDataChanges();
                    }
                }
                else
                {
                    if (!this.ContainsKey(key))
                    {
                        Add(key, value);
                    }
                }
            };
            if (Wire != null && value.Wire == null)
            {
                var subWire = Wire.Child(key, true);
                value.MakeRealtime(subWire);
                subWire.InvokeStart();
            }
            return (key, value);
        }

        protected FirebaseObject ObjectFactory()
        {
            return new FirebaseObject();
        }

        public void MakeRealtime(RealtimeWire wire)
        {
            wire.OnStart += delegate
            {
                if (!wire.InvokeSetFirst) Clear();

                var subWires = new Dictionary<string, RealtimeWire>();

                var path = wire.Query.GetAbsolutePath();
                path = path.Last() == '/' ? path : path + "/";
                var separatedPath = Utils.SeparateUrl(path);

                var subDatas = wire.App.Database.OfflineDatabase.GetSubDatas(path);

                foreach (var subData in subDatas)
                {
                    var separatedSubPath = Utils.SeparateUrl(subData.Path);
                    var key = separatedSubPath[separatedPath.Length];
                    TryGetValue(key, out FirebaseObject obj);

                    if (obj == null)
                    {
                        obj = ObjectFactory();

                        var subWire = wire.Child(key, false);
                        obj.MakeRealtime(subWire);
                        subWires.Add(key, subWire);

                        Add(key, obj);
                    }
                    else
                    {
                        var subWire = wire.Child(key, wire.InvokeSetFirst);
                        obj.MakeRealtime(subWire);
                        subWires.Add(key, subWire);
                    }
                }

                foreach (var obj in this)
                {
                    if (!subWires.ContainsKey(obj.Key))
                    {
                        var subWire = wire.Child(obj.Key, wire.InvokeSetFirst);
                        obj.Value.MakeRealtime(subWire);
                        subWires.Add(obj.Key, subWire);
                    }
                }

                Wire = wire;

                foreach (var subWire in subWires.Values)
                {
                    subWire.InvokeStart();
                }
            };
            wire.OnStop += delegate
            {
                Wire = null;
                foreach (var prop in this)
                {
                    prop.Value.Wire.InvokeStop();
                }
            };
            wire.OnStream += streamObject =>
            {
                bool hasChanges = false;
                try
                {
                    if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                    else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                    else if (streamObject.Path.Length == 1)
                    {
                        var props = new (string, StreamData)[0];

                        if (streamObject.Object is MultiStreamData multi) props = multi.Data.Select(i => (i.Key, i.Value)).ToArray();
                        else if (streamObject.Object is null) props = new (string, StreamData)[0];

                        var hasSubChanges = ReplaceObjects(props,
                            args =>
                            {
                                var subStreamObject = new StreamObject(args.value, args.key);
                                return args.obj.Wire.InvokeStream(subStreamObject);
                            });
                        if (hasSubChanges) hasChanges = true;
                    }
                    else if (streamObject.Path.Length == 2)
                    {
                        var props = new (string, StreamData)[0];

                        if (streamObject.Object is MultiStreamData multi) props = new (string, StreamData)[] { (streamObject.Path[1], multi) };
                        else if (streamObject.Object is null) props = new (string, StreamData)[] { (streamObject.Path[1], null) };

                        var hasSubChanges = UpdateObjects(props,
                            args =>
                            {
                                var subStreamObject = new StreamObject(args.value, args.key);
                                return args.obj.Wire.InvokeStream(subStreamObject);
                            });
                        if (hasSubChanges) hasChanges = true;
                    }
                    else if (streamObject.Path.Length == 3)
                    {
                        var props = new (string, StreamData)[0];

                        if (streamObject.Object is SingleStreamData single) props = new (string, StreamData)[] { (streamObject.Path[2], single) };
                        else if (streamObject.Object is null) props = new (string, StreamData)[] { (streamObject.Path[2], null) };

                        var hasSubChanges = UpdateProperties(streamObject.Path[1], props,
                            args =>
                            {
                                var subStreamObject = new StreamObject(args.value, args.key);
                                return args.property.Wire.InvokeStream(subStreamObject);
                            });
                        if (hasSubChanges) hasChanges = true;
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
                return hasChanges;
            };
        }

        public bool UpdateProperties<T>(string key, IEnumerable<(string key, T value)> properties, Func<(string key, FirebaseProperty property, T value), bool> setter)
        {
            bool hasChanges = false;

            if (!TryGetValue(key, out FirebaseObject obj))
            {
                obj = ValueFactory(key, new FirebaseObject()).value;
                Add(key, obj);
            }

            obj.UpdateProperties(properties, setter);

            return hasChanges;
        }

        public bool ReplaceProperties<T>(string key, IEnumerable<(string key, T value)> properties, Func<(string key, FirebaseProperty property, T value), bool> setter)
        {
            bool hasChanges = false;

            if (!TryGetValue(key, out FirebaseObject obj))
            {
                obj = ValueFactory(key, new FirebaseObject()).value;
                Add(key, obj);
            }

            obj.ReplaceProperties(properties, setter);

            return hasChanges;
        }

        public bool UpdateObjects<T>(IEnumerable<(string key, T value)> objs, Func<(string key, FirebaseObject obj, T value), bool> setter)
        {
            bool hasChanges = false;

            foreach (var data in objs)
            {
                try
                {
                    TryGetValue(data.key, out FirebaseObject obj);

                    if (obj == null)
                    {
                        obj = ObjectFactory();

                        if (EqualityComparer<T>.Default.Equals(data.value, default(T)))
                        {
                            setter.Invoke((data.key, obj, default));
                        }
                        else
                        {
                            if (Wire != null)
                            {
                                var subWire = Wire.Child(data.key, false);
                                obj.MakeRealtime(subWire);
                                subWire.InvokeStart();
                            }

                            setter.Invoke((data.key, obj, data.value));

                            Add(data.key, obj);

                            hasChanges = true;
                        }
                    }
                    else
                    {
                        if (EqualityComparer<T>.Default.Equals(data.value, default(T)))
                        {
                            setter.Invoke((data.key, obj, default));
                            Remove(data.key);
                            hasChanges = true;
                        }
                        else
                        {
                            if (setter.Invoke((data.key, obj, data.value)))
                            {
                                hasChanges = true;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }

            return hasChanges;
        }

        public bool ReplaceObjects<T>(IEnumerable<(string key, T value)> objs, Func<(string key, FirebaseObject obj, T value), bool> setter)
        {
            bool hasChanges = false;

            var excluded = new List<KeyValuePair<string, FirebaseObject>>(this.Where(i => !objs.Any(j => j.key == i.Key)));

            foreach (var prop in excluded)
            {
                if (setter.Invoke((prop.Key, prop.Value, default)))
                {
                    Remove(prop.Key);
                    hasChanges = true;
                }
            }

            if (UpdateObjects(objs, setter)) hasChanges = true;

            return hasChanges;
        }

        public bool Delete()
        {
            var hasChanges = false;
            foreach (var prop in new Dictionary<string, FirebaseObject>(this))
            {
                prop.Value.Delete();
                Remove(prop.Key);
                hasChanges = true;
            }
            return hasChanges;
        }

        #endregion
    }
}
