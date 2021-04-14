using RestfulFirebase.Common.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    #region Helpers

    public class PropertyHolder
    {
        public DistinctProperty Property { get; set; }
        public string Group { get; set; }
        public string PropertyName { get; set; }
    }

    #endregion

    public class ObservableObject : IObservableAttributed
    {
        #region Properties

        public AttributeHolder Holder { get; } = new AttributeHolder();

        private PropertyChangedEventHandler PropertyChangedHandler
        {
            get => Holder.GetAttribute<PropertyChangedEventHandler>(nameof(PropertyChangedHandler), nameof(ObservableObject), delegate { }).Value;
            set => Holder.SetAttribute(nameof(PropertyChangedHandler), nameof(ObservableObject), value);
        }

        private EventHandler<ContinueExceptionEventArgs> PropertyErrorHandler
        {
            get => Holder.GetAttribute<EventHandler<ContinueExceptionEventArgs>>(nameof(PropertyErrorHandler), nameof(ObservableObject), delegate { }).Value;
            set => Holder.SetAttribute(nameof(PropertyErrorHandler), nameof(ObservableObject), value);
        }

        protected List<PropertyHolder> PropertyHolders
        {
            get => Holder.GetAttribute(nameof(PropertyHolders), nameof(ObservableObject), new List<PropertyHolder>()).Value;
            set => Holder.SetAttribute(nameof(PropertyHolders), nameof(ObservableObject), value);
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (this)
                {
                    PropertyChangedHandler += value;
                }
            }
            remove
            {
                lock (this)
                {
                    PropertyChangedHandler -= value;
                }
            }
        }

        public event EventHandler<ContinueExceptionEventArgs> PropertyError
        {
            add
            {
                lock (this)
                {
                    PropertyErrorHandler += value;
                }
            }
            remove
            {
                lock (this)
                {
                    PropertyErrorHandler -= value;
                }
            }
        }

        #endregion

        #region Initializers

        public static ObservableObject CreateFromProperties(IEnumerable<(string Key, string Blob)> properties)
        {
            var obj = new ObservableObject(null);
            foreach (var prop in properties)
            {
                try
                {
                    bool hasChanges = false;

                    var propHolder = obj.PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(prop.Key));

                    if (propHolder == null)
                    {
                        propHolder = new PropertyHolder()
                        {
                            Property = obj.PropertyFactory(DistinctProperty.CreateFromKeyAndBlob(prop.Key, prop.Blob)),
                            Group = null,
                            PropertyName = null
                        };
                        obj.PropertyHolders.Add(propHolder);
                        hasChanges = true;
                    }
                    else
                    {
                        if (propHolder.Property.Blob != prop.Blob)
                        {
                            if (propHolder.Property.UpdateBlob(prop.Blob)) hasChanges = true;
                        }
                    }

                    if (hasChanges) obj.OnChanged(PropertyChangeType.Set, propHolder.Property.Key, propHolder.Group, propHolder.PropertyName);
                }
                catch (Exception ex)
                {
                    obj.OnError(ex);
                }
            }
            return obj;
        }

        public ObservableObject(IAttributed attributed)
        {
            Holder.Initialize(this, attributed);
            foreach (var property in this
                .GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                property.GetValue(this);
            }
        }

        #endregion

        #region Methods

        protected virtual void OnChanged(
            PropertyChangeType type,
            string key,
            string group,
            string propertyName) => PropertyChangedHandler?.Invoke(this, new ObservableObjectInternalChangesEventArgs(type, key, group, propertyName));

        public virtual void OnError(Exception exception, bool defaultIgnoreAndContinue = true)
        {
            var args = new ContinueExceptionEventArgs(exception, defaultIgnoreAndContinue);
            PropertyErrorHandler?.Invoke(this, args);
            if (!args.IgnoreAndContinue)
            {
                throw args.Exception;
            }
        }

        public virtual void OnError(ContinueExceptionEventArgs args)
        {
            PropertyErrorHandler?.Invoke(this, args);
            if (!args.IgnoreAndContinue)
            {
                throw args.Exception;
            }
        }

        protected virtual DistinctProperty PropertyFactory<T>(T property)
            where T : DistinctProperty
        {
            return property;
        }

        protected virtual bool SetProperty<T>(
            T value,
            string key,
            string group = null,
            [CallerMemberName] string propertyName = null,
            Func<T, T, bool> validateValue = null,
            Func<(T value, DistinctProperty property), bool> customValueSetter = null)
        {
            PropertyHolder propHolder = null;
            bool hasChanges = false;

            try
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(key));
                var newData = DataTypeConverter.GetConverter<T>().Encode(value);

                if (propHolder != null)
                {
                    var existingValue = propHolder.Property.ParseValue<T>();

                    if (propHolder.Group != group)
                    {
                        propHolder.Group = group;
                        hasChanges = true;
                    }

                    if (propHolder.PropertyName != propertyName)
                    {
                        propHolder.PropertyName = propertyName;
                        hasChanges = true;
                    }

                    if (propHolder.Property.GetData() != newData ||
                        (validateValue?.Invoke(existingValue, value) ?? false))
                    {
                        if (customValueSetter == null)
                        {
                            if (propHolder.Property.UpdateData(newData)) hasChanges = true;
                        }
                        else
                        {
                            if (customValueSetter.Invoke((value, propHolder.Property))) hasChanges = true;
                        }
                    }
                }
                else
                {
                    propHolder = new PropertyHolder()
                    {
                        Property = PropertyFactory(DistinctProperty.CreateFromKey(key)),
                        Group = group,
                        PropertyName = propertyName
                    };
                    if (customValueSetter == null) propHolder.Property.UpdateData(newData);
                    else customValueSetter.Invoke((value, propHolder.Property));
                    PropertyHolders.Add(propHolder);
                    hasChanges = true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
                return hasChanges;
            }

            if (hasChanges) OnChanged(PropertyChangeType.Set, propHolder.Property.Key, propHolder.Group, propHolder.PropertyName);
            return hasChanges;
        }

        protected virtual T GetProperty<T>(
            string key,
            string group = null,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            Func<(T value, DistinctProperty property), bool> customValueSetter = null)
        {
            bool hasChanges = false;
            var propHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(key));
            if (propHolder == null)
            {
                propHolder = new PropertyHolder()
                {
                    Property = PropertyFactory(DistinctProperty.CreateFromKey(key)),
                    Group = group,
                    PropertyName = propertyName
                };
                if (customValueSetter == null) propHolder.Property.UpdateData(DataTypeConverter.GetConverter<T>().Encode(defaultValue));
                else customValueSetter.Invoke((defaultValue, propHolder.Property));
                PropertyHolders.Add(propHolder);
                hasChanges = true;
            }
            else
            {
                if (propHolder.Group != group)
                {
                    propHolder.Group = group;
                    hasChanges = true;
                }

                if (propHolder.PropertyName != propertyName)
                {
                    propHolder.PropertyName = propertyName;
                    hasChanges = true;
                }
            }

            if (hasChanges) OnChanged(PropertyChangeType.Set, propHolder.Property.Key, propHolder.Group, propHolder.PropertyName);
            return propHolder.Property.ParseValue<T>();
        }

        protected virtual void DeleteProperty(string key)
        {
            var propHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(key));
            if (propHolder == null) return;
            bool hasChanges = false;
            if (propHolder.Property.Blob != null)
            {
                propHolder.Property.UpdateBlob(null);
                hasChanges = true;
            }
            if (hasChanges) OnChanged(PropertyChangeType.Set, propHolder.Property.Key, propHolder.Group, propHolder.PropertyName);
        }

        public IEnumerable<DistinctProperty> GetRawProperties(string group = null)
        {
            return group == null ?
                PropertyHolders
                    .Select(i => i.Property) :
                PropertyHolders
                    .Where(i => i.Group == group)
                    .Select(i => i.Property);
        }

        #endregion
    }
}
