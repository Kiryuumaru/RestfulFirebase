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

        private List<(DistinctProperty Model, string Group, string PropertyName)> Properties
        {
            get => GetAttribute(nameof(Properties), nameof(ObservableObject), new List<(DistinctProperty Model, string Group, string PropertyName)>()).Value;
            set => SetAttribute(nameof(Properties), nameof(ObservableObject), value);
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

        protected virtual bool SetProperty<T>(T value, string key, string group = "", [CallerMemberName] string propertyName = "", Action onChanged = null, Func<T, T, bool> validateValue = null)
        {
            try
            {
                var existingCell = Properties.FirstOrDefault(i => i.Model.Key.Equals(key)).Model;
                var newCell = DistinctProperty.CreateFromKeyAndValue(key, value);

                if (existingCell != null)
                {
                    //if value didn't change
                    if (existingCell.Data?.Equals(newCell.Data) ?? newCell.Data == null)
                        return false;

                    var existingValue = existingCell.ParseValue<T>();

                    //if value changed but didn't validate
                    if (validateValue != null && !(validateValue?.Invoke(existingValue, value) ?? false))
                        return false;

                    existingCell.Update(newCell);
                }
                else
                {
                    Properties.Add((newCell, group, propertyName));
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

        protected virtual T GetProperty<T>(string key, string group = "", T defaultValue = default, [CallerMemberName] string propertyName = "")
        {
            var (Model, Group, PropertyName) = Properties.FirstOrDefault(i => i.Model.Key.Equals(key) && i.Group.Equals(group));
            if (Model == null)
            {
                Properties.Add((DistinctProperty.CreateFromKeyAndValue(key, defaultValue), group, propertyName));
                return defaultValue;
            }
            return Model.ParseValue<T>();
        }

        protected virtual void DeleteProperty(string key)
        {
            var (Model, Group, PropertyName) = Properties.FirstOrDefault(i => i.Model.Key.Equals(key));
            if (Model == null) return;
            Properties.RemoveAll(i => i.Model.Key.Equals(key));
            OnChanged(PropertyChangeType.Delete, key, Group, PropertyName);
        }

        public IEnumerable<DistinctProperty> GetRawProperties(string group = null)
        {
            return group == null ? Properties.Select(i => i.Model) : Properties.FindAll(i => i.Group.Equals(group)).Select(i => i.Model);
        }

        public void PatchRawProperties(IEnumerable<DistinctProperty> properties, string group = null)
        {
            var groupProperties = group == null ? Properties : Properties.FindAll(i => i.Group.Equals(group));
            foreach (var property in properties)
            {
                try
                {
                    var (ExistingModel, ExistingGroup, ExistingPropertyName) = groupProperties.FirstOrDefault(i => i.Model.Key.Equals(property.Key));

                    if (ExistingModel != null)
                    {
                        //if value didn't change
                        if (ExistingModel.Data?.Equals(property.Data) ?? property.Data == null)
                            continue;

                        ExistingModel.Update(property);
                    }
                    else
                    {
                        ExistingModel = property;
                        Properties.Add((ExistingModel, ExistingGroup, ExistingPropertyName));
                    }

                    OnChanged(PropertyChangeType.Set, ExistingModel.Key, ExistingGroup, ExistingPropertyName);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }
        }

        public void PatchRawProperties(IEnumerable<(DistinctProperty Model, string Group, string PropertyName)> properties)
        {
            foreach (var (Model, Group, PropertyName) in properties)
            {
                try
                {
                    var (ExistingModel, ExistingGroup, ExistingPropertyName) = Properties.FirstOrDefault(i => i.Model.Key.Equals(Model.Key) && i.Group.Equals(Group));

                    if (ExistingModel != null)
                    {
                        //if value didn't change
                        if (ExistingModel.Data?.Equals(Model.Data) ?? Model.Data == null)
                            continue;

                        ExistingModel.Update(Model);
                    }
                    else
                    {
                        ExistingModel = Model;
                        Properties.Add((Model, ExistingGroup, ExistingPropertyName));
                    }

                    OnChanged(PropertyChangeType.Set, ExistingModel.Key, ExistingGroup, ExistingPropertyName);
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
