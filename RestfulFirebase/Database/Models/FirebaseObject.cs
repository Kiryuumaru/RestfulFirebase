using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObject : ObservableObject, IRealtimeModel
    {
        #region Properties

        public RealtimeWire Wire
        {
            get => Holder.GetAttribute<RealtimeWire>();
            set => Holder.SetAttribute(value);
        }

        public string Key
        {
            get => Holder.GetAttribute<string>();
            set => Holder.SetAttribute(value);
        }

        #endregion

        #region Initializers

        public FirebaseObject(IAttributed attributed)
            : base(attributed)
        {

        }

        public FirebaseObject(string key)
            : base()
        {
            Key = key;
        }

        #endregion

        #region Methods

        protected override PropertyHolder PropertyFactory(string key, string group, string propertyName)
        {
            var newObj = new FirebaseProperty(key);
            return new PropertyHolder()
            {
                Property = newObj,
                Key = newObj.Key,
                Group = group,
                PropertyName = propertyName
            };
        }

        public void SetPersistableProperty<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            Func<T, T, bool> validateValue = null,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            base.SetProperty(value, key, nameof(FirebaseObject), propertyName, validateValue, customValueSetter);
        }

        public T GetPersistableProperty<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            Func<(T value, ObservableProperty property), bool> customValueSetter = null)
        {
            return base.GetProperty(key, nameof(FirebaseObject), defaultValue, propertyName, customValueSetter);
        }

        public IEnumerable<FirebaseProperty> GetRawPersistableProperties()
        {
            return GetRawProperties(nameof(FirebaseObject)).Select(i => (FirebaseProperty)i);
        }

        public void MakeRealtime(RealtimeWire wire)
        {
            wire.OnStart += delegate
            {
                Wire = wire;
                foreach (var prop in GetRawPersistableProperties())
                {
                    var subWire = Wire.Child(prop.Key, Wire.InvokeSetFirst);
                    prop.MakeRealtime(subWire);
                    subWire.InvokeStart();
                }
            };
            wire.OnStop += delegate
            {
                Wire = null;
                foreach (var prop in GetRawPersistableProperties())
                {
                    prop.Wire.InvokeStop();
                }
            };
            wire.OnStream += streamObject =>
            {
                bool hasChanges = false;
                try
                {
                    if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                    else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                    else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                    else if (streamObject.Path.Length == 1)
                    {
                        var props = new (string, StreamData)[0];

                        if (streamObject.Object is MultiStreamData multi) props = multi.Data.Select(i => (i.Key, i.Value)).ToArray();
                        else if(streamObject.Object is SingleStreamData single) props = new (string, StreamData)[] { (streamObject.Path[1], single) };
                        else if (streamObject.Object is null) props = new (string, StreamData)[0];

                        var hasSubChanges = ReplaceProperties(props,
                            args =>
                            {
                                var subStreamObject = new StreamObject(args.value, args.property.Key);
                                return args.property.Wire.InvokeStream(subStreamObject);
                            });
                        if (hasSubChanges) hasChanges = true;
                    }
                    else if (streamObject.Path.Length == 2)
                    {
                        var props = new (string, StreamData)[0];

                        if (streamObject.Object is MultiStreamData multi) props = new (string, StreamData)[] { (streamObject.Path[1], multi) };
                        else if (streamObject.Object is SingleStreamData single) props = new (string, StreamData)[] { (streamObject.Path[1], single) };
                        else if (streamObject.Object is null) props = new (string, StreamData)[] { (streamObject.Path[1], null) };

                        var hasSubChanges = UpdateProperties(props,
                            args =>
                            {
                                var subStreamObject = new StreamObject(args.value, args.property.Key);
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

        public bool UpdateProperties<T>(IEnumerable<(string key, T value)> properties, Func<(FirebaseProperty property, T value), bool> setter)
        {
            bool hasChanges = false;

            foreach (var prop in properties)
            {
                try
                {
                    bool hasSubChanges = false;

                    var propHolder = PropertyHolders.FirstOrDefault(i => i.Key.Equals(prop.key));

                    if (propHolder == null)
                    {
                        propHolder = PropertyFactory(prop.key, null, null);

                        if (Wire != null)
                        {
                            var subWire = Wire.Child(propHolder.Key, false);
                            ((FirebaseProperty)propHolder.Property).MakeRealtime(subWire);
                            subWire.InvokeStart();
                        }

                        setter.Invoke(((FirebaseProperty)propHolder.Property, prop.value));

                        PropertyHolders.Add(propHolder);

                        hasSubChanges = true;
                    }
                    else
                    {
                        if (setter.Invoke(((FirebaseProperty)propHolder.Property, prop.value)))
                        {
                            hasSubChanges = true;
                        }
                    }

                    if (hasSubChanges)
                    {
                        OnChanged(propHolder.Key, propHolder.Group, propHolder.PropertyName);
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

        public bool ReplaceProperties<T>(IEnumerable<(string key, T value)> properties, Func<(FirebaseProperty property, T value), bool> setter)
        {
            bool hasChanges = false;

            var excluded = new List<PropertyHolder>(PropertyHolders.Where(i => !properties.Any(j => j.key == i.Key)));

            foreach (var propHolder in excluded)
            {
                if (setter.Invoke(((FirebaseProperty)propHolder.Property, default)))
                {
                    OnChanged(propHolder.Key, propHolder.Group, propHolder.PropertyName);
                    hasChanges = true;
                }
            }

            if (UpdateProperties(properties, setter)) hasChanges = true;

            return hasChanges;
        }

        public bool Delete()
        {
            var hasChanges = false;
            foreach (var propHolder in PropertyHolders)
            {
                if (DeleteProperty(propHolder.Key)) hasChanges = true;
            }
            return hasChanges;
        }

        public T ParseModel<T>()
            where T : FirebaseObject
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        #endregion
    }
}
