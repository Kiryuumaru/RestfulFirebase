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
    public class ObservableProperty : AttributeHolder, INotifyPropertyChanged
    {
        #region Properties

        private PropertyChangedEventHandler PropertyChangedHandler
        {
            get => GetAttribute<PropertyChangedEventHandler>(nameof(PropertyChangedHandler), nameof(ObservableProperty), delegate { }).Value;
            set => SetAttribute(nameof(PropertyChangedHandler), nameof(ObservableProperty), value);
        }

        private EventHandler<ObservableExceptionEventArgs> PropertyErrorHandler
        {
            get => GetAttribute<EventHandler<ObservableExceptionEventArgs>>(nameof(PropertyErrorHandler), nameof(ObservableProperty), delegate { }).Value;
            set => SetAttribute(nameof(PropertyErrorHandler), nameof(ObservableProperty), value);
        }

        public IEnumerable<byte> Bytes
        {
            get => GetAttribute<IEnumerable<byte>>(nameof(Bytes), nameof(ObservableProperty)).Value;
            private set => SetAttribute(nameof(Bytes), nameof(ObservableProperty), value);
        }

        public string Data { get => Bytes == null ? null : Encoding.Unicode.GetString(Bytes.ToArray()); }

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

        public static ObservableProperty Create()
        {
            return new ObservableProperty(null);
        }

        public static ObservableProperty CreateFromValue<T>(T value)
        {
            return DataTypeDecoder.GetDecoder<T>().Parse(value);
        }

        public static ObservableProperty CreateFromData(string data)
        {
            var obj = new ObservableProperty(null);
            obj.Update(data);
            return obj;
        }

        public static ObservableProperty CreateFromBytes(IEnumerable<byte> bytes)
        {
            var obj = new ObservableProperty(null);
            obj.Update(bytes);
            return obj;
        }

        public ObservableProperty(AttributeHolder holder) : base(holder)
        {

        }

        #endregion

        #region Methods

        protected virtual void OnChanged(string propertyName = "") => PropertyChangedHandler?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual void OnError(Exception exception) => PropertyErrorHandler?.Invoke(this, new ObservableExceptionEventArgs(exception));

        public void Update(string data)
        {
            try
            {
                Bytes = Encoding.Unicode.GetBytes(data);
                OnChanged(nameof(Bytes));
                OnChanged(nameof(Data));
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
                Bytes = bytes;
                OnChanged(nameof(Bytes));
                OnChanged(nameof(Data));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public T ParseValue<T>()
        {
            return DataTypeDecoder.GetDecoder<T>().Parse(this);
        }

        public T Parse<T>()
            where T : ObservableObject
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        #endregion
    }
}
