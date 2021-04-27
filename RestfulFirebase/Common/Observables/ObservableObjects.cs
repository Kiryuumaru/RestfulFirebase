using RestfulFirebase.Common.Converters;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Observables
{
    #region Helpers

    public class PropertyHolder
    {
        public ObservableObject Property { get; set; }
        public string Key { get; set; }
        public string Group { get; set; }
        public string PropertyName { get; set; }
    }

    #endregion

    public class ObservableObjects : IObservable
    {
        #region Properties

        public AttributeHolder Holder { get; } = new AttributeHolder();

        private PropertyChangedEventHandler PropertyChangedHandler
        {
            get => Holder.GetAttribute<PropertyChangedEventHandler>(delegate { });
            set => Holder.SetAttribute(value);
        }

        private EventHandler<ContinueExceptionEventArgs> PropertyErrorHandler
        {
            get => Holder.GetAttribute<EventHandler<ContinueExceptionEventArgs>>(delegate { });
            set => Holder.SetAttribute(value);
        }

        protected List<PropertyHolder> PropertyHolders
        {
            get => Holder.GetAttribute<List<PropertyHolder>>(new List<PropertyHolder>());
            set => Holder.SetAttribute(value);
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

        public ObservableObjects(IAttributed attributed)
        {
            Holder.Inherit(attributed);
        }

        public ObservableObjects()
            : this(null)
        {
            foreach (var property in this
                .GetType()
                .GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                property.GetValue(this);
            }
        }

        #endregion

        #region Methods

        public virtual void OnChanged(
            PropertyChangeType type,
            string key,
            string group,
            string propertyName) => PropertyChangedHandler?.Invoke(this, new ObservableObjectChangesEventArgs(type, key, group, propertyName));

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

        protected virtual bool SetProperty<T>(
            T value,
            string key,
            string group = null,
            [CallerMemberName] string propertyName = null,
            Func<T, T, bool> validateValue = null,
            Func<(T value, ObservableObject property), bool> customValueSetter = null)
        {
            PropertyHolder propHolder = null;
            bool hasChanges = false;

            try
            {
                propHolder = PropertyHolders.FirstOrDefault(i => i.Key.Equals(key));

                if (propHolder != null)
                {
                    var existingValue = propHolder.Property.GetValue<T>();

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

                    if (validateValue?.Invoke(existingValue, value) ?? true)
                    {
                        if (customValueSetter == null)
                        {
                            if (propHolder.Property.SetValue(value)) hasChanges = true;
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
                        Property = new ObservableObject<T>(),
                        Key = key,
                        Group = group,
                        PropertyName = propertyName
                    };
                    if (customValueSetter == null) propHolder.Property.SetValue(value);
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

            if (hasChanges) OnChanged(PropertyChangeType.Set, propHolder.Key, propHolder.Group, propHolder.PropertyName);
            return hasChanges;
        }

        protected virtual T GetProperty<T>(
            string key,
            string group = null,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            Func<(T value, ObservableObject property), bool> customValueSetter = null)
        {
            bool hasChanges = false;
            var propHolder = PropertyHolders.FirstOrDefault(i => i.Key.Equals(key));

            if (propHolder == null)
            {
                propHolder = new PropertyHolder()
                {
                    Property = new ObservableObject<T>(),
                    Key = key,
                    Group = group,
                    PropertyName = propertyName
                };
                if (customValueSetter == null) propHolder.Property.SetValue(defaultValue);
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

            if (hasChanges) OnChanged(PropertyChangeType.Set, propHolder.Key, propHolder.Group, propHolder.PropertyName);
            return propHolder.Property.GetValue<T>();
        }

        protected virtual void DeleteProperty(string key)
        {
            var propHolder = PropertyHolders.FirstOrDefault(i => i.Key.Equals(key));
            if (propHolder == null) return;
            bool hasChanges = propHolder.Property.SetBlob(null);
            if (hasChanges) OnChanged(PropertyChangeType.Set, propHolder.Key, propHolder.Group, propHolder.PropertyName);
        }

        protected IEnumerable<ObservableObject> GetRawProperties(string group = null)
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
