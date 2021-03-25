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

        private const string DataKey = "d";
        private const string AdditionsKey = "a";

        public AttributeHolder Holder { get; } = new AttributeHolder();

        private PropertyChangedEventHandler PropertyChangedHandler
        {
            get => Holder.GetAttribute<PropertyChangedEventHandler>(nameof(PropertyChangedHandler), nameof(ObservableProperty), delegate { }).Value;
            set => Holder.SetAttribute(nameof(PropertyChangedHandler), nameof(ObservableProperty), value);
        }

        private EventHandler<ContinueExceptionEventArgs> PropertyErrorHandler
        {
            get => Holder.GetAttribute<EventHandler<ContinueExceptionEventArgs>>(nameof(PropertyErrorHandler), nameof(ObservableProperty), delegate { }).Value;
            set => Holder.SetAttribute(nameof(PropertyErrorHandler), nameof(ObservableProperty), value);
        }

        public string Data
        {
            get => Holder.GetAttribute<string>(nameof(Data), nameof(ObservableProperty)).Value;
            private set => Holder.SetAttribute(nameof(Data), nameof(ObservableProperty), value);
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

        public static ObservableProperty Create()
        {
            return new ObservableProperty(null);
        }

        public static ObservableProperty CreateFromValue<T>(T value)
        {
            return CreateFromData(DataTypeDecoder.GetDecoder<T>().Encode(value));
        }

        public static ObservableProperty CreateFromData(string data)
        {
            var obj = new ObservableProperty(null);
            obj.Update(data);
            return obj;
        }

        public ObservableProperty(IAttributed attributed)
        {
            Holder.Initialize(this, attributed);
        }

        #endregion

        #region Methods

        protected virtual void OnChanged(PropertyChangeType propertyChangeType, string propertyName = "") => PropertyChangedHandler?.Invoke(this, new ObservablePropertyChangesEventArgs(propertyChangeType, propertyName));

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

        public Type GetDataType()
        {
            return DataTypeDecoder.GetDataType(Data);
        }

        public void Update(string data)
        {
            try
            {
                Data = data;
                OnChanged(PropertyChangeType.Set, nameof(Data));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void Null()
        {
            try
            {
                Data = default;
                OnChanged(PropertyChangeType.Delete, nameof(Data));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public T ParseValue<T>()
        {
            if (GetDataType() != typeof(T)) throw new Exception("Data type mismatch");
            return DataTypeDecoder.GetDecoder<T>().Decode(Data);
        }

        public T Parse<T>()
            where T : ObservableObject
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        #endregion
    }
}
