using RestfulFirebase.Common.Conversions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    #region EventArgs

    public enum PropertyChangeType
    {
        Set, Delete
    }

    public class PersistancePropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public string Key { get; }
        public PropertyChangeType Type { get; }
        public string KeyGroup { get; }
        public PersistancePropertyChangedEventArgs(string key, PropertyChangeType type, string propertyName = "", string group = "") : base(propertyName)
        {
            Key = key;
            Type = type;
            KeyGroup = group;
        }
    }

    #endregion

    public abstract class ObservableObject : INotifyPropertyChanged
    {
        #region Properties

        private readonly List<(string PropertyName, string Group, ObservableProperty Model)> properties = new List<(string PropertyName, string Group, ObservableProperty Model)>();

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Initializers

        protected ObservableObject() { }
        protected ObservableObject(IEnumerable<ObservableProperty> properties)
        {
            this.properties = properties.Select(i => ("", "", i)).ToList();
        }
        protected ObservableObject(IEnumerable<(string PropertyName, string Group, ObservableProperty Model)> properties)
        {
            this.properties = properties.ToList();
        }

        #endregion

        #region Methods

        protected virtual void OnPropertyChanged(string key, PropertyChangeType type, [CallerMemberName] string propertyName = "", string group = "") => PropertyChanged?.Invoke(this, new PersistancePropertyChangedEventArgs(key, type, propertyName, group));

        public virtual bool SetProperty<T>(T value, string key, [CallerMemberName] string propertyName = "", string group = "", Action onChanged = null, Func<T, T, bool> validateValue = null)
        {
            ObservableProperty existingCell = properties.FirstOrDefault(i => i.Model.Key.Equals(key)).Model;
            ObservableProperty newCell = ObservableProperty.CreateDerived(value, key);

            if (existingCell != null)
            {
                //if value didn't change
                if (existingCell.Data.Equals(newCell.Data))
                    return false;

                try
                {
                    var existingValue = existingCell.ParseValue<T>();

                    //if value changed but didn't validate
                    if (validateValue != null && !validateValue(existingValue, value))
                        return false;
                }
                catch { }

                existingCell.Update(newCell);
            }
            else
            {
                properties.Add((propertyName, group, newCell));
            }

            onChanged?.Invoke();
            OnPropertyChanged(key, PropertyChangeType.Set, propertyName, group);
            return true;
        }

        public virtual T GetProperty<T>(string key)
        {
            var (Group, PropertyName, Model) = properties.FirstOrDefault(i => i.Model.Key.Equals(key));
            if (Model == null) return default;
            return Model.ParseValue<T>();
        }

        public virtual void DeleteProperty(string key)
        {
            var (PropertyName, Group, Model) = properties.FirstOrDefault(i => i.Model.Key.Equals(key));
            if (Model == null) return;
            properties.RemoveAll(i => i.Model.Key.Equals(key));
            OnPropertyChanged(key, PropertyChangeType.Delete, PropertyName, Group);
        }

        public IEnumerable<ObservableProperty> GetRawProperties(string group = null)
        {
            return group == null ? properties.Select(i => i.Model) : properties.FindAll(i => i.Group?.Equals(group) ?? false).Select(i => i.Model);
        }

        #endregion
    }
}
