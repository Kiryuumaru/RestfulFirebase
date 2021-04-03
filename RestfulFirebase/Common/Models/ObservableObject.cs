using RestfulFirebase.Common.Conversions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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


    public class ObservableObject : IAttributed, INotifyPropertyChanged
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

        public static ObservableObject CreateFromProperties(IEnumerable<(string Key, string Data)> properties)
        {
            var obj = new ObservableObject(null);
            obj.UpdateRawProperties(properties);
            return obj;
        }

        public ObservableObject(IAttributed attributed)
        {
            Holder.Initialize(this, attributed);
        }

        #endregion

        #region Methods

        protected virtual void OnChanged(
            DistinctProperty newProperty,
            PropertyChangeType type,
            string key,
            string group,
            string propertyName) => PropertyChangedHandler?.Invoke(this, new ObservableObjectInternalChangesEventArgs(newProperty, type, key, group, propertyName));

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
            Action<DistinctProperty> onInternalChanged = null)
        {
            DistinctProperty newProperty = null;
            try
            {
                var propHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(key));
                newProperty = PropertyFactory(DistinctProperty.CreateFromKeyAndValue(key, value));

                if (propHolder != null)
                {
                    var existingValue = propHolder.Property.ParseValue<T>();

                    bool hasChanges = false;

                    if (propHolder.Group != group && group != null)
                    {
                        propHolder.Group = group;
                        hasChanges = true;
                    }

                    if (propHolder.PropertyName != propertyName && propertyName != null)
                    {
                        propHolder.PropertyName = propertyName;
                        hasChanges = true;
                    }

                    if (propHolder.Property.Data != newProperty.Data ||
                        (validateValue?.Invoke(existingValue, value) ?? false))
                    {
                        propHolder.Property.Update(newProperty.Data);
                        hasChanges = true;
                    }

                    if (!hasChanges) return false;
                }
                else
                {
                    PropertyHolders.Add(new PropertyHolder()
                    {
                        Property = newProperty,
                        Group = group,
                        PropertyName = propertyName
                    });
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
                return false;
            }

            onInternalChanged?.Invoke(newProperty);
            OnChanged(newProperty, PropertyChangeType.Set, key, group, propertyName);
            return true;
        }

        protected virtual T GetProperty<T>(
            string key,
            string group = null,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null)
        {
            var propertyHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(key));
            if (propertyHolder == null)
            {
                propertyHolder = new PropertyHolder()
                {
                    Property = PropertyFactory(DistinctProperty.CreateFromKeyAndValue(key, defaultValue)),
                    Group = group,
                    PropertyName = propertyName
                };
                PropertyHolders.Add(propertyHolder);
            }
            else
            {
                if (propertyHolder.Group != group && group != null)
                {
                    propertyHolder.Group = group;
                }

                if (propertyHolder.PropertyName != propertyName && propertyName != null)
                {
                    propertyHolder.PropertyName = propertyName;
                }
            }

            return propertyHolder.Property.ParseValue<T>();
        }

        protected virtual void DeleteProperty(string key)
        {
            var propertyHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(key));
            if (propertyHolder == null) return;
            propertyHolder.Property.Null();
            OnChanged(propertyHolder.Property, PropertyChangeType.Delete, key, propertyHolder.Group, propertyHolder.PropertyName);
        }

        protected virtual void DeleteProperties(string group)
        {
            foreach (var propertyHolder in new List<PropertyHolder>(PropertyHolders.Where(i => i.Group == group)))
            {
                propertyHolder.Property.Null();
                OnChanged(propertyHolder.Property, PropertyChangeType.Delete, propertyHolder.Property.Key, propertyHolder.Group, propertyHolder.PropertyName);
            }
        }

        public void UpdateRawProperties(IEnumerable<(string Key, string Data)> properties, Action<PropertyHolder> perItemFollowup = null)
        {
            foreach (var property in properties)
            {
                try
                {
                    var propHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(property.Key));

                    if (propHolder == null)
                    {
                        propHolder = new PropertyHolder()
                        {
                            Property = PropertyFactory(DistinctProperty.CreateFromKeyAndData(property.Key, property.Data)),
                            Group = null,
                            PropertyName = null
                        };
                        PropertyHolders.Add(propHolder);
                    }
                    else
                    {
                        bool hasChanges = false;

                        if (propHolder.Property.Data != property.Data)
                        {
                            propHolder.Property.Update(property.Data);
                            hasChanges = true;
                        }

                        if (!hasChanges) continue;
                    }

                    perItemFollowup?.Invoke(propHolder);
                    OnChanged(propHolder.Property, PropertyChangeType.Set, propHolder.Property.Key, propHolder.Group, propHolder.PropertyName);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
        }

        public void ReplaceRawProperties(IEnumerable<(string Key, string Data)> properties, Action<PropertyHolder> perItemFollowup = null)
        {
            foreach (var propHolder in PropertyHolders.Where(i => !properties.Any(j => j.Key == i.Property.Key)))
            {
                perItemFollowup?.Invoke(propHolder);
                propHolder.Property.Null();
            }
            UpdateRawProperties(properties, perItemFollowup);
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

        public T Parse<T>()
            where T : ObservableObject
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        #endregion
    }
}
