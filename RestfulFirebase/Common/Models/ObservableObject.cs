using RestfulFirebase.Common.Conversions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class ObservableObject : AttributeHolder, INotifyPropertyChanged
    {
        #region Helpers

        private class PropertyHolder
        {
            public DistinctProperty Property { get; set; }
            public string Group { get; set; }
            public string PropertyName { get; set; }
        }

        #endregion

        #region Properties

        private PropertyChangedEventHandler PropertyChangedHandler
        {
            get => GetAttribute<PropertyChangedEventHandler>(nameof(PropertyChangedHandler), nameof(ObservableObject), delegate { }).Value;
            set => SetAttribute(nameof(PropertyChangedHandler), nameof(ObservableObject), value);
        }

        private EventHandler<ObservableExceptionEventArgs> PropertyErrorHandler
        {
            get => GetAttribute<EventHandler<ObservableExceptionEventArgs>>(nameof(PropertyErrorHandler), nameof(ObservableObject), delegate { }).Value;
            set => SetAttribute(nameof(PropertyErrorHandler), nameof(ObservableObject), value);
        }

        private List<PropertyHolder> PropertyHolders
        {
            get => GetAttribute(nameof(PropertyHolders), nameof(ObservableObject), new List<PropertyHolder>()).Value;
            set => SetAttribute(nameof(PropertyHolders), nameof(ObservableObject), value);
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

        public event EventHandler<ObservableExceptionEventArgs> PropertyError
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

        public static ObservableObject CreateFromProperties(IEnumerable<DistinctProperty> properties)
        {
            var obj = new ObservableObject(null);
            obj.PatchRawProperties(properties);
            return obj;
        }

        public static ObservableObject CreateFromProperties(IEnumerable<(DistinctProperty Model, string Group, string PropertyName)> properties)
        {
            var obj = new ObservableObject(null);
            obj.PatchRawProperties(properties);
            return obj;
        }

        public ObservableObject(AttributeHolder holder) : base(holder)
        {

        }

        #endregion

        #region Methods

        protected virtual void OnChanged(PropertyChangeType type, string key, string group, string propertyName) => PropertyChangedHandler?.Invoke(this, new ObservableObjectChangesEventArgs(type, key, group, propertyName));
        
        protected virtual void OnError(Exception exception) => PropertyErrorHandler?.Invoke(this, new ObservableExceptionEventArgs(exception));

        protected virtual bool SetProperty<T>(T value, string key, string group = null, [CallerMemberName] string propertyName = null, Action onChanged = null, Func<T, T, bool> validateValue = null)
        {
            try
            {
                var existingHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(key));
                var newHolder = new PropertyHolder()
                {
                    Property = DistinctProperty.CreateFromKeyAndValue(key, value),
                    Group = group,
                    PropertyName = propertyName
                };

                if (existingHolder != null)
                {
                    var existingValue = existingHolder.Property.ParseValue<T>();

                    bool hasChanges = false;

                    if (existingHolder.Group != newHolder.Group && newHolder.Group != null)
                    {
                        existingHolder.Group = newHolder.Group;
                        hasChanges = true;
                    }

                    if (existingHolder.PropertyName != newHolder.PropertyName && newHolder.PropertyName != null)
                    {
                        existingHolder.PropertyName = newHolder.PropertyName;
                        hasChanges = true;
                    }

                    if (existingHolder.Property.Data != newHolder.Property.Data ||
                        (validateValue?.Invoke(existingValue, value) ?? false))
                    {
                        existingHolder.Property.Update(newHolder.Property);
                        hasChanges = true;
                    }

                    if (!hasChanges) return false;
                }
                else
                {
                    PropertyHolders.Add(newHolder);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            onChanged?.Invoke();
            OnChanged(PropertyChangeType.Set, key, group, propertyName);
            return true;
        }

        protected virtual T GetProperty<T>(string key, string group = null, T defaultValue = default, [CallerMemberName] string propertyName = null)
        {
            var propertyHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(key));
            if (propertyHolder == null)
            {
                propertyHolder = new PropertyHolder()
                {
                    Property = DistinctProperty.CreateFromKeyAndValue(key, defaultValue),
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
            PropertyHolders.RemoveAll(i => i.Property.Key.Equals(key));
            OnChanged(PropertyChangeType.Delete, key, propertyHolder.Group, propertyHolder.PropertyName);
        }

        public IEnumerable<DistinctProperty> GetRawProperties(string group = null)
        {
            return group == null ? PropertyHolders.Select(i => i.Property) : PropertyHolders.FindAll(i => i.Group == group).Select(i => i.Property);
        }

        public void PatchRawProperties(IEnumerable<DistinctProperty> properties, string group = null)
        {
            var groupProperties = group == null ? PropertyHolders : PropertyHolders.FindAll(i => i.Group.Equals(group));
            foreach (var property in properties)
            {
                try
                {
                    var existingHolder = groupProperties.FirstOrDefault(i => i.Property.Key.Equals(property.Key));
                    var newHolder = new PropertyHolder()
                    {
                        Property = property,
                        Group = group,
                        PropertyName = null
                    };

                    if (existingHolder != null)
                    {
                        bool hasChanges = false;

                        if (existingHolder.Group != newHolder.Group && newHolder.Group != null)
                        {
                            existingHolder.Group = newHolder.Group;
                            hasChanges = true;
                        }

                        if (existingHolder.PropertyName != newHolder.PropertyName && newHolder.PropertyName != null)
                        {
                            existingHolder.PropertyName = newHolder.PropertyName;
                            hasChanges = true;
                        }

                        if (existingHolder.Property.Data != newHolder.Property.Data)
                        {
                            existingHolder.Property.Update(newHolder.Property);
                            hasChanges = true;
                        }

                        if (!hasChanges) continue;
                    }
                    else
                    {
                        PropertyHolders.Add(newHolder);
                    }

                    OnChanged(PropertyChangeType.Set, existingHolder.Property.Key, existingHolder.Group, existingHolder.PropertyName);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
        }

        public void PatchRawProperties(IEnumerable<(DistinctProperty Property, string Group, string PropertyName)> properties)
        {
            foreach (var (Property, Group, PropertyName) in properties)
            {
                try
                {
                    var existingHolder = PropertyHolders.FirstOrDefault(i => i.Property.Key.Equals(Property.Key));
                    var newHolder = new PropertyHolder()
                    {
                        Property = Property,
                        Group = Group,
                        PropertyName = PropertyName
                    };

                    if (existingHolder != null)
                    {
                        bool hasChanges = false;

                        if (existingHolder.Group != newHolder.Group && newHolder.Group != null)
                        {
                            existingHolder.Group = newHolder.Group;
                            hasChanges = true;
                        }

                        if (existingHolder.PropertyName != newHolder.PropertyName && newHolder.PropertyName != null)
                        {
                            existingHolder.PropertyName = newHolder.PropertyName;
                            hasChanges = true;
                        }

                        if (existingHolder.Property.Data != newHolder.Property.Data)
                        {
                            existingHolder.Property.Update(newHolder.Property);
                            hasChanges = true;
                        }

                        if (!hasChanges) continue;
                    }
                    else
                    {
                        PropertyHolders.Add(newHolder);
                    }

                    OnChanged(PropertyChangeType.Set, existingHolder.Property.Key, existingHolder.Group, existingHolder.PropertyName);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
        }

        public T Parse<T>()
            where T : ObservableObject
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        #endregion
    }
}
