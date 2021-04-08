using RestfulFirebase.Common.Converters;
using RestfulFirebase.Common.Converters.Additionals;
using RestfulFirebase.Common.Converters.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class ObservableProperty : PrimitiveBlob, INotifyPropertyChanged
    {
        #region Properties

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

        public static new ObservableProperty Create()
        {
            return new ObservableProperty(null);
        }

        public static new ObservableProperty CreateFromValue<T>(T value)
        {
            var encoded = DataTypeConverter.GetConverter<T>().Encode(value);
            var data = Helpers.SerializeString(encoded, null);
            return CreateFromBlob(data);
        }

        public static new ObservableProperty CreateFromBlob(string blob)
        {
            var obj = new ObservableProperty(null);
            obj.UpdateBlob(blob);
            return obj;
        }

        public ObservableProperty(IAttributed attributed)
            : base(attributed)
        {

        }

        #endregion

        #region Methods

        protected virtual void OnChanged(string propertyName = "") => PropertyChangedHandler?.Invoke(this, new PropertyChangedEventArgs(propertyName));

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

        public new T GetAdditional<T>(string key)
        {
            try
            {
                return base.GetAdditional<T>(key);
            }
            catch (Exception ex)
            {
                OnError(ex);
                return default;
            }
        }

        public new void SetAdditional<T>(string key, T value)
        {
            try
            {
                base.SetAdditional(key, value);
                OnChanged(nameof(Blob));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public new void DeleteAdditional(string key)
        {
            try
            {
                base.DeleteAdditional(key);
                OnChanged(nameof(Blob));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public new void ClearAdditionals()
        {
            try
            {
                base.ClearAdditionals();
                OnChanged(nameof(Blob));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public new void UpdateBlob(string blob)
        {
            try
            {
                base.UpdateBlob(blob);
                OnChanged(nameof(Blob));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public new void UpdateData(string data)
        {
            try
            {
                base.UpdateData(data);
                OnChanged(nameof(Blob));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        #endregion
    }
}
