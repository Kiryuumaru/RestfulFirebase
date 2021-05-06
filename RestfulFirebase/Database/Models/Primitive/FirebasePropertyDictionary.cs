using RestfulFirebase.Common;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Models.Primitive;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Models.Primitive
{
    public class FirebasePropertyDictionary : ObservableDictionary<string, FirebaseProperty>, IRealtimeModel
    {
        #region Properties

        public RealtimeWire Wire
        {
            get => Holder.GetAttribute<RealtimeWire>();
            set => Holder.SetAttribute(value);
        }

        #endregion

        #region Initializers

        public FirebasePropertyDictionary(IAttributed attributed)
            : base(attributed)
        {

        }

        public FirebasePropertyDictionary()
            : base(null)
        {

        }

        #endregion

        #region Methods

        protected override (string key, FirebaseProperty value) ValueFactory(string key, FirebaseProperty value)
        {
            if (Wire != null && value.Wire == null)
            {
                var subWire = Wire.Child(key, true);
                value.MakeRealtime(subWire);
                subWire.InvokeStart();
            }
            return (key, value);
        }

        protected FirebaseProperty PropertyFactory()
        {
            return new FirebaseProperty();
        }

        public void MakeRealtime(RealtimeWire wire)
        {
            wire.OnStart += delegate
            {
                if (!wire.InvokeSetFirst) Clear();

                var path = wire.Query.GetAbsolutePath();
                path = path.Last() == '/' ? path : path + "/";
                var separatedPath = Helpers.SeparateUrl(path);

                var subDatas = wire.App.Database.OfflineDatabase.GetSubDatas(path);

                foreach (var subData in subDatas)
                {
                    var separatedSubPath = Helpers.SeparateUrl(subData.Path);
                    var key = separatedSubPath[separatedPath.Length];
                    TryGetValue(key, out FirebaseProperty prop);

                    if (prop == null)
                    {
                        prop = PropertyFactory();
                        prop.SetBlob(wire.InvokeSetFirst ? null : subData.Blob);
                        prop.MakeRealtime(wire.Child(key, true));
                        Add(key, prop);
                    }
                }

                Wire = wire;

                foreach (var prop in this)
                {
                    if (prop.Value.Wire == null)
                    {
                        var subWire = Wire.Child(prop.Key, Wire.InvokeSetFirst);
                        prop.Value.MakeRealtime(subWire);
                        subWire.InvokeStart();
                    }
                    else
                    {
                        prop.Value.Wire.InvokeStart();
                    }
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

                        var hasSubChanges = ReplaceProperties(props,
                            args =>
                            {
                                var subStreamObject = new StreamObject(args.value, args.key);
                                return args.property.Wire.InvokeStream(subStreamObject);
                            });
                        if (hasSubChanges) hasChanges = true;
                    }
                    else if (streamObject.Path.Length == 2)
                    {
                        var props = new (string, StreamData)[0];

                        if (streamObject.Object is SingleStreamData single) props = new (string, StreamData)[] { (streamObject.Path[1], single) };
                        else if (streamObject.Object is null) props = new (string, StreamData)[] { (streamObject.Path[1], null) };

                        var hasSubChanges = UpdateProperties(props,
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

        public bool UpdateProperties<T>(IEnumerable<(string key, T value)> properties, Func<(string key, FirebaseProperty property, T value), bool> setter)
        {
            bool hasChanges = false;

            foreach (var data in properties)
            {
                try
                {
                    TryGetValue(data.key, out FirebaseProperty prop);

                    if (prop == null)
                    {
                        prop = PropertyFactory();

                        if (EqualityComparer<T>.Default.Equals(data.value, default(T)))
                        {
                            setter.Invoke((data.key, prop, default));
                        }
                        else
                        {
                            if (Wire != null)
                            {
                                var subWire = Wire.Child(data.key, false);
                                prop.MakeRealtime(subWire);
                                subWire.InvokeStart();
                            }

                            setter.Invoke((data.key, prop, data.value));

                            Add(data.key, prop);

                            hasChanges = true;
                        }
                    }
                    else
                    {
                        if (EqualityComparer<T>.Default.Equals(data.value, default(T)))
                        {
                            setter.Invoke((data.key, prop, default));
                            Remove(data.key);
                            hasChanges = true;
                        }
                        else
                        {
                            if (setter.Invoke((data.key, prop, data.value)))
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

        public bool ReplaceProperties<T>(IEnumerable<(string key, T value)> properties, Func<(string key, FirebaseProperty property, T value), bool> setter)
        {
            bool hasChanges = false;

            var excluded = new List<KeyValuePair<string, FirebaseProperty>>(this.Where(i => !properties.Any(j => j.key == i.Key)));

            foreach (var prop in excluded)
            {
                if (setter.Invoke((prop.Key, prop.Value, default)))
                {
                    Remove(prop.Key);
                    hasChanges = true;
                }
            }

            if (UpdateProperties(properties, setter)) hasChanges = true;

            return hasChanges;
        }

        public bool Delete()
        {
            var hasChanges = false;
            foreach (var prop in new Dictionary<string, FirebaseProperty>(this))
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
