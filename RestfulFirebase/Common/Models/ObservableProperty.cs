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
    public class ObservableProperty : PrimitiveBlob, IObservableAttributed
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
            return new ObservableProperty(PrimitiveBlob.CreateFromValue(value));
        }

        public static new ObservableProperty CreateFromBlob(string blob)
        {
            return new ObservableProperty(PrimitiveBlob.CreateFromBlob(blob));
        }

        public static new ObservableProperty CreateFromData(string data)
        {
            return new ObservableProperty(PrimitiveBlob.CreateFromData(data));
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

        public new bool SetAdditional<T>(string key, T value, string tag = null)
        {
            try
            {
                if (base.SetAdditional(key, value, tag))
                {
                    OnChanged(nameof(Blob));
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public new bool DeleteAdditional(string key, string tag = null)
        {
            try
            {
                if (base.DeleteAdditional(key, tag))
                {
                    OnChanged(nameof(Blob));
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public new bool ClearAdditionals(string tag = null)
        {
            try
            {
                if (base.ClearAdditionals(tag))
                {
                    OnChanged(nameof(Blob));
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public new bool UpdateBlob(string blob, string tag = null)
        {
            try
            {
                if (base.UpdateBlob(blob, tag))
                {
                    OnChanged(nameof(Blob));
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public new bool UpdateData(string data, string tag = null)
        {
            try
            {
                if (base.UpdateData(data, tag))
                {
                    OnChanged(nameof(Blob));
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        #endregion
    }
}
