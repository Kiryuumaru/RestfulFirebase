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
    public class ObservableProperty : PrimitiveData, INotifyPropertyChanged
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
            var encoded = DataTypeDecoder.GetDecoder<T>().Encode(value);
            var data = Helpers.SerializeString(encoded, null);
            return CreateFromData(data);
        }

        public static new ObservableProperty CreateFromData(string data)
        {
            var obj = new ObservableProperty(null);
            obj.Update(data);
            return obj;
        }

        public ObservableProperty(IAttributed attributed)
            : base(attributed)
        {

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

        public new string GetAdditional(string key)
        {
            try
            {
                return base.GetAdditional(key);
            }
            catch (Exception ex)
            {
                OnError(ex);
                return null;
            }
        }

        public new void SetAdditional(string key, string data)
        {
            try
            {
                base.SetAdditional(key, data);
                OnChanged(PropertyChangeType.Set, true, nameof(Data));
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
                OnChanged(PropertyChangeType.Delete, true, nameof(Data));
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
                OnChanged(PropertyChangeType.Delete, true, nameof(Data));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public new void Update(string data)
        {
            try
            {
                base.Update(data);
                OnChanged(PropertyChangeType.Set, false, nameof(Data));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        public new void Null()
        {
            try
            {
                base.Null();
                OnChanged(PropertyChangeType.Delete, false, nameof(Data));
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        #endregion
    }
}
