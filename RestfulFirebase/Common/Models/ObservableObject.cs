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

        public event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                lock (this)
                {
                    var handler = (PropertyChangedEventHandler)GetAttribute(nameof(PropertyChanged), nameof(ObservableObject)).Value;
                    handler += value;
                }
            }
            remove
            {
                lock (this)
                {
                    var handler = (PropertyChangedEventHandler)GetAttribute(nameof(PropertyChanged), nameof(ObservableObject)).Value;
                    handler -= value;
                }
            }
        }

        public event EventHandler<ObservableExceptionEventArgs> PropertyError
        {
            add
            {
                lock (this)
                {
                    var handler = (EventHandler<ObservableExceptionEventArgs>)GetAttribute(nameof(PropertyError), nameof(ObservableObject)).Value;
                    handler += value;
                }
            }
            remove
            {
                lock (this)
                {
                    var handler = (EventHandler<ObservableExceptionEventArgs>)GetAttribute(nameof(PropertyError), nameof(ObservableObject)).Value;
                    handler -= value;
                }
            }
        }

        private List<(string PropertyName, string Group, DistinctProperty Model)> Properties
        {
            get
            {
                var properties = (List<(string PropertyName, string Group, DistinctProperty Model)>)GetAttribute(nameof(Properties), nameof(ObservableObject)).Value;
                if (properties == null)
                {
                    properties = new List<(string PropertyName, string Group, DistinctProperty Model)>();
                    SetAttribute(nameof(Properties), nameof(ObservableObject), properties);
                }
                return properties;
            }
            set => SetAttribute(nameof(Properties), nameof(ObservableObject), value);
        }

        #endregion

        #region Initializers

        public static ObservableObject CreateFromProperties(IEnumerable<DistinctProperty> properties)
        {
            var obj = new ObservableObject(null)
            {
                Properties = properties.Select(i => ("", "", i)).ToList()
            };
            return obj;
        }

        public static ObservableObject CreateFromProperties(IEnumerable<(string PropertyName, string Group, DistinctProperty Model)> properties)
        {
            var obj = new ObservableObject(null)
            {
                Properties = properties.ToList()
            };
            return obj;
        }

        public ObservableObject(AttributeHolder holder) : base(holder)
        {

        }

        #endregion

        #region Methods

        protected virtual void OnChanged(PropertyChangeType type, string key, string propertyName = "", string group = "")
        {
            var handler = (PropertyChangedEventHandler)GetAttribute(nameof(PropertyChanged), nameof(ObservableObject)).Value;
            handler?.Invoke(this, new ObservableObjectChangesEventArgs(type, key, propertyName, group));
        }

        protected virtual void OnError(Exception exception)
        {
            var handler = (EventHandler<ObservableExceptionEventArgs>)GetAttribute(nameof(PropertyError), nameof(ObservableObject)).Value;
            handler?.Invoke(this, new ObservableExceptionEventArgs(exception));
        }

        protected virtual bool SetProperty<T>(T value, string key, string propertyName = "", string group = "", Action onChanged = null, Func<T, T, bool> validateValue = null)
        {
            try
            {
                DistinctProperty existingCell = Properties.FirstOrDefault(i => i.Model.Key.Equals(key)).Model;
                DistinctProperty newCell = DistinctProperty.CreateFromKeyAndValue(key, value);

                if (existingCell != null)
                {
                    //if value didn't change
                    if (existingCell.Data.Equals(newCell.Data))
                        return false;

                    var existingValue = existingCell.ParseValue<T>();

                    //if value changed but didn't validate
                    if (validateValue != null && !validateValue(existingValue, value))
                        return false;

                    existingCell.Update(newCell);
                }
                else
                {
                    Properties.Add((propertyName, group, newCell));
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }

            onChanged?.Invoke();
            OnChanged(PropertyChangeType.Set, key, propertyName, group);
            return true;
        }

        protected virtual T GetProperty<T>(string key)
        {
            var (PropertyName, Group, Model) = Properties.FirstOrDefault(i => i.Model.Key.Equals(key));
            if (Model == null) return default;
            return Model.ParseValue<T>();
        }

        protected virtual void DeleteProperty(string key)
        {
            var (PropertyName, Group, Model) = Properties.FirstOrDefault(i => i.Model.Key.Equals(key));
            if (Model == null) return;
            Properties.RemoveAll(i => i.Model.Key.Equals(key));
            OnChanged(PropertyChangeType.Delete, key, PropertyName, Group);
        }

        public IEnumerable<DistinctProperty> GetRawProperties(string group = null)
        {
            return group == null ? Properties.Select(i => i.Model) : Properties.FindAll(i => i.Group?.Equals(group) ?? false).Select(i => i.Model);
        }

        public T Parse<T>()
            where T : ObservableObject
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        #endregion
    }
}
