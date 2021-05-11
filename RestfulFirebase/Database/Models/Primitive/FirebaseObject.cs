using ObservableHelpers.Observables;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Database.Models.Primitive
{
    public class FirebaseObject : ObservableObject, IRealtimeModel
    {
        #region Properties

        public RealtimeWire Wire { get; private set; }

        #endregion

        #region Methods

        protected override PropertyHolder PropertyFactory(string key, string propertyName, string group, bool serializable)
        {
            ObservableProperty prop;
            if (serializable) prop = new FirebaseProperty();
            else prop = new ObservableNonSerializableProperty();
            return new PropertyHolder()
            {
                Property = prop,
                Key = key,
                PropertyName = propertyName,
                Group = group
            };
        }

        public void SetPersistableProperty<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            Func<T, T, bool> validateValue = null,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            base.SetPropertyWithKey(value, key, propertyName, nameof(FirebaseObject), true, validateValue, customValueSetter);
        }

        public T GetPersistableProperty<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            return base.GetPropertyWithKey(key, defaultValue, propertyName, nameof(FirebaseObject), true, customValueSetter);
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
                    var key = separatedSubPath[separatedPath.Length];

                    PropertyHolder propHolder = null;
                    lock(PropertyHolders)
                    {
                        propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
                    }

                    if (propHolder == null)
                    {
                        propHolder = PropertyFactory(key, null, nameof(FirebaseObject), true);

                        var subWire = wire.Child(key, false);
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

                foreach (var propHolder in GetRawPersistableProperties())
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
                    if (!Wire.InvokeSetFirst) OnChanged(subWire.propHolder.Key, subWire.propHolder.PropertyName, subWire.propHolder.Group);
                }
            };
            wire.OnStop += delegate
            {
                Wire = null;
                foreach (var prop in GetRawPersistableProperties())
                {
                    ((FirebaseProperty)prop.Property).Wire.InvokeStop();
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
                    bool hasSubChanges = false;

                    PropertyHolder propHolder = null;
                    lock (PropertyHolders)
                    {
                        propHolder = PropertyHolders.FirstOrDefault(i => i.Key.Equals(data.key));
                    }

                    if (propHolder == null)
                    {
                        propHolder = PropertyFactory(data.key, null, null, true);

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

                        hasSubChanges = true;
                    }
                    else
                    {
                        if (setter.Invoke((propHolder.Key, (FirebaseProperty)propHolder.Property, data.value)))
                        {
                            hasSubChanges = true;
                        }
                    }

                    if (hasSubChanges)
                    {
                        OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
                        hasChanges = true;
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
                    OnChanged(propHolder.Key, propHolder.PropertyName, propHolder.Group);
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
