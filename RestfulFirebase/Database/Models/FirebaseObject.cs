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
    public class FirebaseObject : ObservableObject, IRealtimeModelProxy
    {
        #region Properties

        internal RealtimeModelWire ModelWire { get; private set; }

        #endregion

        #region Methods

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

        public virtual void Dispose()
        {
            ModelWire?.Unsubscribe();
            ModelWire = null;
        }

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

        void IRealtimeModelProxy.StartRealtime(RealtimeModelWire modelWire, bool invokeSetFirst)
        {
            if (ModelWire != null)
            {
                ModelWire?.Unsubscribe();
                ModelWire = null;
            }

            ModelWire = modelWire;

            ModelWire.Subscribe();

            ModelWire.SetOnChanges(args =>
            {
                if (!string.IsNullOrEmpty(args.Path))
                {
                    var separated = Utils.UrlSeparate(args.Path);
                    var key = separated[0];
                    var wireBlob = ModelWire.RealtimeInstance.Child(key).GetBlob();
                    PropertyHolder propHolder = null;
                    lock (PropertyHolders)
                    {
                        propHolder = PropertyHolders.FirstOrDefault(i => i.Key == key);
                    }
                    if (propHolder == null && wireBlob != null)
                    {
                        propHolder = PropertyFactory(key, null, nameof(FirebaseObject));
                        ModelWire.RealtimeInstance.Child(key).SubModel((FirebaseProperty)propHolder.Property);
                        lock (PropertyHolders)
                        {
                            PropertyHolders.Add(propHolder);
                        }
                    }
                    OnChangedWithKey(key);
                }
            });

            InitializeProperties(false);

            List<PropertyHolder> props = new List<PropertyHolder>();
            lock (PropertyHolders)
            {
                props = GetRawProperties(nameof(FirebaseObject)).ToList();
            }
            var paths = ModelWire.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

            foreach (var prop in props)
            {
                if (invokeSetFirst) ModelWire.RealtimeInstance.Child(prop.Key).PutModel((FirebaseProperty)prop.Property);
                else ModelWire.RealtimeInstance.Child(prop.Key).SubModel((FirebaseProperty)prop.Property);
                paths.RemoveAll(i => i == prop.Key);
            }

            foreach (var path in paths)
            {
                lock (PropertyHolders)
                {
                    if (PropertyHolders.Any(i => i.Key == path)) continue;
                }
                var propHolder = PropertyFactory(path, null, nameof(FirebaseObject));
                ModelWire.RealtimeInstance.Child(path).SubModel((FirebaseProperty)propHolder.Property);
                lock (PropertyHolders)
                {
                    PropertyHolders.Add(propHolder);
                }
            }
        }

        #endregion
    }
}
