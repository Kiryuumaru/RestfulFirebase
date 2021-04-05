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
            var encoded = DataTypeDecoder.GetDecoder<T>().Encode(value);
            var data = Helpers.SerializeString(encoded, null);
            return CreateFromData(data);
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

        protected virtual void OnChanged(
            PropertyChangeType propertyChangeType,
            bool isAdditionals,
            string propertyName = "") => PropertyChangedHandler?.Invoke(this, new ObservablePropertyChangesEventArgs(propertyChangeType, isAdditionals, propertyName));

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

        public void SetAdditional(string key, string data)
        {
            try
            {
                var deserialized = Helpers.DeserializeString(Data);
                if (deserialized == null) deserialized = new string[2];
                if (deserialized.Length != 2) throw new Exception("Data length error");
                deserialized[1] = Helpers.BlobSetValue(deserialized[1], key, data);
                Data = Helpers.SerializeString(deserialized);
                OnChanged(PropertyChangeType.Set, true, nameof(Data));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public string GetAdditional(string key)
        {
            try
            {
                var deserialized = Helpers.DeserializeString(Data);
                if (deserialized == null) return null;
                if (deserialized.Length != 2) throw new Exception("Data length error");
                return Helpers.BlobGetValue(deserialized[1], key);
            }
            catch (Exception ex)
            {
                OnError(ex);
                return null;
            }
        }

        public void DeleteAdditional(string key)
        {
            try
            {
                var deserialized = Helpers.DeserializeString(Data);
                if (deserialized == null) return;
                if (deserialized.Length != 2) throw new Exception("Data length error");
                deserialized[1] = Helpers.BlobDeleteValue(deserialized[1], key);
                Data = Helpers.SerializeString(deserialized);
                OnChanged(PropertyChangeType.Delete, true, nameof(Data));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void ClearAdditionals()
        {
            try
            {
                var deserialized = Helpers.DeserializeString(Data);
                if (deserialized == null) return;
                if (deserialized.Length != 2) throw new Exception("Data length error");
                else
                {
                    deserialized[1] = null;
                    Data = Helpers.SerializeString(deserialized);
                    OnChanged(PropertyChangeType.Delete, true, nameof(Data));
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public void Update(string data)
        {
            try
            {
                Data = data;
                OnChanged(PropertyChangeType.Set, false, nameof(Data));
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
                OnChanged(PropertyChangeType.Delete, false, nameof(Data));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public bool IsNull()
        {
            return Data == default;
        }

        public T ParseValue<T>()
        {
            var deserialized = Helpers.DeserializeString(Data);
            if (deserialized == null) return default;
            if (deserialized.Length != 2) throw new Exception("Data length error");
            return DataTypeDecoder.GetDecoder<T>().Decode(deserialized[0]);
        }

        public T Parse<T>()
            where T : ObservableObject
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        #endregion
    }
}
