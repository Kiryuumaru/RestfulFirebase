using RestfulFirebase.Common.Conversions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public enum PropertyChangeType
    {
        Set, Delete
    }

    public class ObjectChangedEventArgs : PropertyChangedEventArgs
    {
        public string Key { get; }
        public PropertyChangeType Type { get; }
        public string KeyGroup { get; }
        public ObjectChangedEventArgs(PropertyChangeType type, string key, string propertyName = "", string group = "") : base(propertyName)
        {
            Type = type;
            Key = key;
            KeyGroup = group;
        }
    }

    public class ObjectExceptionEventArgs : EventArgs
    {
        public Exception Exception { get; }
        public ObjectExceptionEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }

    public class ObservableObjectHolder
    {
        private List<(string PropertyName, string Group, DistinctProperty Model)> properties = new List<(string PropertyName, string Group, DistinctProperty Model)>();

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<ObjectExceptionEventArgs> PropertyError;

        public class ObservableObject : INotifyPropertyChanged
        {
            public ObservableObjectHolder Holder { get; } = new ObservableObjectHolder();

            public event PropertyChangedEventHandler PropertyChanged
            {
                add => Holder.PropertyChanged += value;
                remove => Holder.PropertyChanged -= value;
            }

            public event EventHandler<ObjectExceptionEventArgs> PropertyError
            {
                add => Holder.PropertyError += value;
                remove => Holder.PropertyError -= value;
            }

            public void OnChanged(PropertyChangeType type, string key, [CallerMemberName] string propertyName = "", string group = "") => Holder.PropertyChanged?.Invoke(this, new ObjectChangedEventArgs(type, key, propertyName, group));

            public void OnError(Exception exception) => Holder.PropertyError?.Invoke(this, new ObjectExceptionEventArgs(exception));

            public ObservableObject() { }

            public ObservableObject(ObservableObjectHolder holder)
            {
                Holder = holder;
            }

            public ObservableObject(IEnumerable<DistinctProperty> properties)
            {
                Holder.properties = properties.Select(i => ("", "", i)).ToList();
            }

            public ObservableObject(IEnumerable<(string PropertyName, string Group, DistinctProperty Model)> properties)
            {
                Holder.properties = properties.ToList();
            }

            public IEnumerable<DistinctProperty> GetRawProperties(string group = null)
            {
                return group == null ? Holder.properties.Select(i => i.Model) : Holder.properties.FindAll(i => i.Group?.Equals(group) ?? false).Select(i => i.Model);
            }

            protected virtual bool SetProperty<T>(T value, string key, [CallerMemberName] string propertyName = "", string group = "", Action onChanged = null, Func<T, T, bool> validateValue = null)
            {
                try
                {
                    DistinctProperty existingCell = Holder.properties.FirstOrDefault(i => i.Model.Key.Equals(key)).Model;
                    DistinctProperty newCell = DistinctProperty.CreateDerived(value, key);

                    if (existingCell != null)
                    {
                        //if value didn't change
                        if (existingCell.Holder.Data.Equals(newCell.Holder.Data))
                            return false;

                        var existingValue = existingCell.ParseValue<T>();

                        //if value changed but didn't validate
                        if (validateValue != null && !validateValue(existingValue, value))
                            return false;

                        existingCell.Update(newCell);
                    }
                    else
                    {
                        Holder.properties.Add((propertyName, group, newCell));
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
                var (PropertyName, Group, Model) = Holder.properties.FirstOrDefault(i => i.Model.Key.Equals(key));
                if (Model == null) return default;
                return Model.ParseValue<T>();
            }

            protected virtual void DeleteProperty(string key)
            {
                var (PropertyName, Group, Model) = Holder.properties.FirstOrDefault(i => i.Model.Key.Equals(key));
                if (Model == null) return;
                Holder.properties.RemoveAll(i => i.Model.Key.Equals(key));
                OnChanged(PropertyChangeType.Delete, key, PropertyName, Group);
            }
        }
    }
}
