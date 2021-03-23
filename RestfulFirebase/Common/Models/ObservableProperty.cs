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
    public class ObservableProperty : IAttributed, INotifyPropertyChanged
    {
        #region Properties

        public AttributeHolder Holder { get; } = new AttributeHolder();

        private PropertyChangedEventHandler PropertyChangedHandler
        {
            get => Holder.GetAttribute<PropertyChangedEventHandler>(nameof(PropertyChangedHandler), nameof(ObservableProperty), delegate { }).Value;
            set => Holder.SetAttribute(nameof(PropertyChangedHandler), nameof(ObservableProperty), value);
        }

        private EventHandler<ObservableExceptionEventArgs> PropertyErrorHandler
        {
            get => Holder.GetAttribute<EventHandler<ObservableExceptionEventArgs>>(nameof(PropertyErrorHandler), nameof(ObservableProperty), delegate { }).Value;
            set => Holder.SetAttribute(nameof(PropertyErrorHandler), nameof(ObservableProperty), value);
        }

        public IEnumerable<byte> Bytes
        {
            get => Holder.GetAttribute<IEnumerable<byte>>(nameof(Bytes), nameof(ObservableProperty)).Value;
            private set => Holder.SetAttribute(nameof(Bytes), nameof(ObservableProperty), value);
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
            return CreateFromData(DataTypeDecoder.GetDecoder<T>().Parse(value));
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

        public ObservableProperty(IAttributed attributed)
        {
            Holder.Initialize(this, attributed);
        }

        #endregion

        #region Methods

        protected virtual void OnChanged(string propertyName = "") => PropertyChangedHandler?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected virtual void OnError(Exception exception) => PropertyErrorHandler?.Invoke(this, new ObservableExceptionEventArgs(exception));

        public void Update(string data)
        {
            try
            {
                if (data != null) Bytes = Encoding.Unicode.GetBytes(data);
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

        public void Empty()
        {
            try
            {
                Bytes = default;
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
            return DataTypeDecoder.GetDecoder<T>().Parse(Data);
        }

        public T Parse<T>()
            where T : ObservableObject
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        #endregion
    }
}
