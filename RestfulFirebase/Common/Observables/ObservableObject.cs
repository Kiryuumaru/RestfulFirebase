using RestfulFirebase.Common.Converters;
using RestfulFirebase.Common.Converters.Additionals;
using RestfulFirebase.Common.Converters.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Observables
{
    public class ObservableObject : ValueHolder, IObservable
    {
        #region Properties

        public object Value { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<ContinueExceptionEventArgs> PropertyError;

        #endregion

        #region Initializers

        public ObservableObject() : base()
        {

        }

        #endregion

        #region Methods

        public virtual void OnChanged(string propertyName = "") => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public virtual void OnError(Exception exception, bool defaultIgnoreAndContinue = true)
        {
            var args = new ContinueExceptionEventArgs(exception, defaultIgnoreAndContinue);
            PropertyError?.Invoke(this, args);
            if (!args.IgnoreAndContinue)
            {
                throw args.Exception;
            }
        }

        public virtual void OnError(ContinueExceptionEventArgs args)
        {
            PropertyError?.Invoke(this, args);
            if (!args.IgnoreAndContinue)
            {
                throw args.Exception;
            }
        }

        public virtual bool SetNull(string tag = null)
        {
            try
            {
                if (Value != null)
                {
                    Value = null;
                    OnChanged("Value");
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public virtual bool SetValue<T>(T value, string tag = null)
        {
            try
            {
                if (Value != (object)value)
                {
                    OnChanged(nameof(Value));
                    return true;
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public virtual T GetValue<T>(T defaultValue = default, string tag = null)
        {
            try
            {
                return (T)Value;
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return defaultValue;
        }

        #endregion
    }
    public class ObservableObject<T> : ObservableObject
    {
        #region Properties



        #endregion

        #region Initializers

        public ObservableObject() : base()
        {

        }

        #endregion

        #region Methods

        public virtual bool SetValue(T value, string tag = null)
        {
            return base.SetValue(value, tag);
        }

        public virtual T GetValue(T defaultValue = default, string tag = null)
        {
            return GetValue<T>(defaultValue, tag);
        }

        #endregion
    }
}
