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

        protected PropertyHolderFactory PropertyHolderFactory
        {
            get => Holder.GetAttribute(nameof(PropertyHolderFactory), nameof(ObservableObject), new PropertyHolderFactory(
                value =>
                {
                    var holder = Holder.GetAttribute(nameof(PropertyHolders), nameof(ObservableObject), new List<PropertyHolder>()).Value;
                    holder.FirstOrDefault(i => i.Property.Key == value.);
                }, key =>
                {
                    var holder = Holder.GetAttribute(nameof(PropertyHolders), nameof(ObservableObject), new List<PropertyHolder>()).Value;
                    holder.FirstOrDefault();
                })).Value;
            set => Holder.SetAttribute(nameof(PropertyHolderFactory), nameof(ObservableObject), value);
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
                        propHolder = new PropertyHolder(
                            obj.PropertyFactory(DistinctProperty.CreateFromKeyAndBlob(prop.Key, prop.Blob)),
                            null,
                            null);
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

                    if (hasChanges) obj.OnChanged(propHolder, PropertyChangeType.Set);
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
            var attr = attributed ?? this;
            foreach (var property in attr
                .GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                property.GetValue(attr);
            }
        }

        #endregion

        #region Methods

        protected virtual void OnChanged(
            PropertyHolder propertyHolder,
            PropertyChangeType type) => PropertyChangedHandler?.Invoke(this, new ObservableObjectInternalChangesEventArgs(propertyHolder, type));

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
            Func<T, T, bool> validateValue = null)
        {
            PropertyHolder propHolder = null;
            bool hasChanges = false;

            try
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(key));
                var newProperty = PropertyFactory(DistinctProperty.CreateFromKeyAndValue(key, value));

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

                    if (propHolder.Property.Blob != newProperty.Blob ||
                        (validateValue?.Invoke(existingValue, value) ?? false))
                    {
                        if (propHolder.Property.UpdateBlob(newProperty.Blob)) hasChanges = true;
                    }
                }
                else
                {
                    propHolder = new PropertyHolder(newProperty, group, propertyName);
                    PropertyHolders.Add(propHolder);
                    hasChanges = true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
                return hasChanges;
            }

            if (hasChanges) OnChanged(propHolder, PropertyChangeType.Set);
            return hasChanges;
        }

        protected virtual T GetProperty<T>(
            string key,
            string group = null,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null)
        {
            bool hasChanges = false;
            var propertyHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(key));
            if (propertyHolder == null)
            {
                propertyHolder = new PropertyHolder(
                    PropertyFactory(DistinctProperty.CreateFromKeyAndValue(key, defaultValue)),
                    group,
                    propertyName);
                PropertyHolders.Add(propertyHolder);
                hasChanges = true;
            }
            else
            {
                if (propertyHolder.Group != group)
                {
                    propertyHolder.Group = group;
                    hasChanges = true;
                }

                if (propertyHolder.PropertyName != propertyName)
                {
                    propertyHolder.PropertyName = propertyName;
                    hasChanges = true;
                }
            }

            if (hasChanges) OnChanged(propertyHolder, PropertyChangeType.Set);
            return propertyHolder.Property.ParseValue<T>();
        }

        protected virtual void DeleteProperty(string key)
        {
            var propertyHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(key));
            if (propertyHolder == null) return;
            bool hasChanges = false;
            if (propertyHolder.Property.Blob != null)
            {
                propertyHolder.Property.UpdateBlob(null);
                hasChanges = true;
            }
            if (hasChanges) OnChanged(propertyHolder, PropertyChangeType.Delete);
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
