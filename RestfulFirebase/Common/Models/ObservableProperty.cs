using RestfulFirebase.Common.Conversions;
using RestfulFirebase.Common.Conversions.Additionals;
using RestfulFirebase.Common.Conversions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class ObservableProperty : Decodable, INotifyPropertyChanged
    {
        public string Key { get; protected set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableProperty(string data, string key) : base(data)
        {
            Key = key;
        }

        public ObservableProperty(IEnumerable<byte> bytes, string key) : base(bytes)
        {
            Key = key;
        }

        protected virtual void OnPropertyChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data)));

        public bool Update(ObservableProperty cellModel)
        {
            if (cellModel.Key.Equals(Key))
            {
                Update(cellModel.Data);
                OnPropertyChanged();
                return true;
            }
            return false;
        }

        public static ObservableProperty CreateDerived<T>(T value, string key)
        {
            var decodable = DataTypeDecoder.GetDecoder<T>().CreateDerived(value);
            return new ObservableProperty(decodable.Data, key);
        }
    }
}
