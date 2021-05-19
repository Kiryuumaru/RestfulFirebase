using ObservableHelpers;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public abstract class FirebaseObject : ObservableObject, IRealtimeModel
    {
        #region Properties

        public RealtimeWire Wire { get; private set; }

        #endregion

        #region Methods

        protected override PropertyHolder PropertyFactory(string key, string propertyName, string group)
        {
            var prop = new FirebaseProperty();
            var propHolder = new PropertyHolder()
            {
                Property = prop,
                Key = key,
                PropertyName = propertyName,
                Group = group
            };
            prop.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(prop.Property))
                {
                    OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                }
            };
            return propHolder;
        }

        public void SetPersistableProperty<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            Func<T, T, bool> validateValue = null,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            base.SetPropertyWithKey(value, key, propertyName, nameof(FirebaseObject), validateValue, customValueSetter);
        }

        public T GetPersistableProperty<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            return base.GetPropertyWithKey(key, defaultValue, propertyName, nameof(FirebaseObject), customValueSetter);
        }

        public IEnumerable<PropertyHolder> GetRawPersistableProperties()
        {
            return GetRawProperties(nameof(FirebaseObject)).Where(i => i.Property is FirebaseProperty);
        }

        public void MakeRealtime(RealtimeWire wire)
        {
            wire.OnStart += delegate
            {
                InitializeProperties(false);

                var subWires = new List<(PropertyHolder propHolder, RealtimeWire wire)>();

                var path = wire.Query.GetAbsolutePath();
                path = path.Last() == '/' ? path : path + "/";
                var separatedPath = Utils.SeparateUrl(path);

                var subDatas = wire.App.Database.OfflineDatabase.GetSubDatas(path);

                foreach (var subData in subDatas)
                {
                    var separatedSubPath = Utils.SeparateUrl(subData.Path);
                    var keys = separatedSubPath.Skip(separatedPath.Length).ToArray();

                    PropertyHolder propHolder = null;
                    lock(PropertyHolders)
                    {
                        propHolder = PropertyHolders.FirstOrDefault(i => i.Key == keys[0]);
                    }

                    if (propHolder == null)
                    {
                        propHolder = PropertyFactory(keys[0], null, nameof(FirebaseObject));

                        var subWire = wire.Child(keys[0], false);
                        ((FirebaseProperty)propHolder.Property).MakeRealtime(subWire);
                        subWires.Add((propHolder, subWire));

                        lock (PropertyHolders)
                        {
                            PropertyHolders.Add(propHolder);
                        }
                    }
                    else
                    {
                        var subWire = wire.Child(propHolder.Key, wire.InvokeSetFirst);
                        ((FirebaseProperty)propHolder.Property).MakeRealtime(subWire);
                        subWires.Add((propHolder, subWire));
                    }
                }

                foreach (var propHolder in GetRawProperties(nameof(FirebaseObject)))
                {
                    if (!subWires.Any(i => i.propHolder.Key == propHolder.Key))
                    {
                        var subWire = wire.Child(propHolder.Key, wire.InvokeSetFirst);
                        ((FirebaseProperty)propHolder.Property).MakeRealtime(subWire);
                        subWires.Add((propHolder, subWire));
                    }
                }

                Wire = wire;

                foreach (var subWire in subWires)
                {
                    subWire.wire.InvokeStart();
                }
            };
            wire.OnStop += delegate
            {
                Wire = null;
                foreach (var propHolder in GetRawProperties(nameof(FirebaseObject)))
                {
                    ((FirebaseProperty)propHolder.Property).Wire.InvokeStop();
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

                        if (streamObject.Object is MultiStreamData multi) props = multi.Data.Select(i => (i.Key, i.Value)).ToArray();
                        else if (streamObject.Object is SingleStreamData single) props = new (string, StreamData)[] { (streamObject.Path[1], single) };
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
                    PropertyHolder propHolder = null;
                    lock (PropertyHolders)
                    {
                        propHolder = PropertyHolders.FirstOrDefault(i => i.Key.Equals(data.key));
                    }

                    if (propHolder == null)
                    {
                        propHolder = PropertyFactory(data.key, null, null);

                        if (Wire != null)
                        {
                            var subWire = Wire.Child(propHolder.Key, false);
                            ((FirebaseProperty)propHolder.Property).MakeRealtime(subWire);
                            subWire.InvokeStart();
                        }

                        setter.Invoke((propHolder.Key, (FirebaseProperty)propHolder.Property, data.value));

                        lock (PropertyHolders)
                        {
                            PropertyHolders.Add(propHolder);
                        }

                        hasChanges = true;
                    }
                    else
                    {
                        if (setter.Invoke((propHolder.Key, (FirebaseProperty)propHolder.Property, data.value)))
                        {
                            hasChanges = true;
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

            List<PropertyHolder> excluded = null;
            lock (PropertyHolders)
            {
                excluded = new List<PropertyHolder>(PropertyHolders.Where(i => !properties.Any(j => j.key == i.Key)));
            }

            foreach (var propHolder in excluded)
            {
                if (setter.Invoke((propHolder.Key, (FirebaseProperty)propHolder.Property, default)))
                {
                    hasChanges = true;
                }
            }

            if (UpdateProperties(properties, setter)) hasChanges = true;

            return hasChanges;
        }

        public bool Delete()
        {
            var hasChanges = false;
            lock (PropertyHolders)
            {
                foreach (var propHolder in PropertyHolders)
                {
                    if (DeleteProperty(propHolder.Key)) hasChanges = true;
                }
            }
            return hasChanges;
        }

        #endregion
    }
}
