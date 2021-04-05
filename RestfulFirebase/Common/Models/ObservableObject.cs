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
        internal PropertyHolder() { }
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
            Func<T, T, bool> validateValue = null,
            Action<(bool HasChanges, PropertyHolder PropertyHolder)> onInternalSet = null)
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
                }
                else
                {
                    propHolder = new PropertyHolder()
                    {
                        Property = newProperty,
                        Group = group,
                        PropertyName = propertyName
                    };
                    PropertyHolders.Add(propHolder);
                    hasChanges = true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
                return hasChanges;
            }

            onInternalSet?.Invoke((hasChanges, propHolder));
            if (hasChanges) OnChanged(propHolder, PropertyChangeType.Set);
            return hasChanges;
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
            OnChanged(propertyHolder, PropertyChangeType.Delete);
        }

        protected virtual void DeleteProperties(string group)
        {
            foreach (var propertyHolder in new List<PropertyHolder>(PropertyHolders.Where(i => i.Group == group)))
            {
                propertyHolder.Property.Null();
                OnChanged(propertyHolder, PropertyChangeType.Delete);
            }
        }

        public void UpdateRawProperties(IEnumerable<(string Key, string Data)> properties, Action<(bool HasChanges, PropertyHolder PropertyHolder)> perItemFollowup = null)
        {
            foreach (var property in properties)
            {
                try
                {
                    bool hasChanges = false;

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
                        hasChanges = true;
                    }
                    else
                    {
                        if (propHolder.Property.Data != property.Data)
                        {
                            propHolder.Property.Update(property.Data);
                            hasChanges = true;
                        }
                    }

                    perItemFollowup?.Invoke((hasChanges, propHolder));
                    if (hasChanges) OnChanged(propHolder, PropertyChangeType.Set);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
        }

        public void ReplaceRawProperties(IEnumerable<(string Key, string Data)> properties, Action<(bool HasChanges, PropertyHolder PropertyHolder)> perItemFollowup = null)
        {
            foreach (var propHolder in PropertyHolders.Where(i => !properties.Any(j => j.Key == i.Property.Key)))
            {
                bool hasChanges = false;
                if (!propHolder.Property.IsNull())
                {
                    propHolder.Property.Null();
                    hasChanges = true;
                }
                perItemFollowup?.Invoke((hasChanges, propHolder));
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
