using RestfulFirebase.Common.Conversions;
using RestfulFirebase.Common.Conversions.Additionals;
using RestfulFirebase.Common.Conversions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class PropertyExceptionEventArgs : PropertyChangedEventArgs
    {
        public Exception Exception { get; }
        public PropertyExceptionEventArgs(Exception exception, string propertyName = "") : base(propertyName)
        {
            Exception = exception;
        }
    }

    public class ObservablePropertyHolder
    {
        public IEnumerable<byte> Bytes { get; private set; }
        public string Data { get => Encoding.Unicode.GetString(Bytes.ToArray()); }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<PropertyExceptionEventArgs> PropertyError;

        public class ObservableProperty : INotifyPropertyChanged
        {
            public ObservablePropertyHolder Holder { get; } = new ObservablePropertyHolder();

            public event PropertyChangedEventHandler PropertyChanged
            {
                add => Holder.PropertyChanged += value;
                remove => Holder.PropertyChanged -= value;
            }

            public event EventHandler<PropertyExceptionEventArgs> PropertyError
            {
                add => Holder.PropertyError += value;
                remove => Holder.PropertyError -= value;
            }

            protected virtual void OnChanged() => Holder.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Data)));

            protected virtual void OnError(Exception exception) => Holder.PropertyError?.Invoke(this, new PropertyExceptionEventArgs(exception, nameof(Data)));

            public ObservableProperty(ObservablePropertyHolder holder)
            {
                Holder = holder;
            }
            public ObservableProperty(string data) => Update(data);

            public ObservableProperty(IEnumerable<byte> bytes) => Update(bytes);

            public void Update(string data)
            {
                try
                {
                    Holder.Bytes = Encoding.Unicode.GetBytes(data);
                    OnChanged();
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }

            public void Update(IEnumerable<byte> bytes)
            {
                try
                {
                    Holder.Bytes = bytes;
                    OnChanged();
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            }

            public static ObservableProperty CreateDerived<T>(T value)
            {
                return DataTypeDecoder.GetDecoder<T>().CreateDerived(value);
            }

            public T ParseValue<T>()
            {
                return DataTypeDecoder.GetDecoder<T>().ParseValue(this);
            }
        }
    }
}
