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

    public class PersistancePropertyChangedEventArgs : PropertyChangedEventArgs
    {
        public string Key { get; }
        public string KeyGroup { get; }
        public PersistancePropertyChangedEventArgs(string key, string propertyName = "", string group = "") : base(propertyName)
        {
            Key = key;
            KeyGroup = group;
        }
    }

    #endregion

    public abstract class ObservableObject : INotifyPropertyChanged
    {
        #region Properties

        private readonly List<CellModel> cellModels = new List<CellModel>();

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Initializers

        protected ObservableObject() { }
        protected ObservableObject(IEnumerable<CellModel> cellModels)
        {
            this.cellModels = new List<CellModel>(cellModels);
        }

        #endregion

        #region Methods

        protected virtual void OnPropertyChanged(string key, string group = "", [CallerMemberName] string propertyName = "") => PropertyChanged?.Invoke(this, new PersistancePropertyChangedEventArgs(key, group, propertyName));

        protected virtual bool SetProperty<T>(T value, string key, [CallerMemberName] string propertyName = "", string group = "", Action onChanged = null, Func<T, T, bool> validateValue = null)
        {
            CellModel existingCell = cellModels.FirstOrDefault(i => i.Key.Equals(key));
            CellModel newCell = CellModel.CreateDerived(value, key);

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
                cellModels.Add(newCell);
            }

            onChanged?.Invoke();
            OnPropertyChanged(key, propertyName, group);
            return true;
        }

        protected virtual T GetProperty<T>(string key)
        {
            var cellModel = cellModels.FirstOrDefault(i => i.Key.Equals(key));
            if (cellModel == null) return default;
            return cellModel.ParseValue<T>();
        }

        public IEnumerable<CellModel> GetCellModels(string group = "")
        {
            var models = new List<CellModel>(cellModels);
            return string.IsNullOrEmpty(group) ? models : models.FindAll(i => i.Group.Equals(group));
        }

        #endregion
    }
}
