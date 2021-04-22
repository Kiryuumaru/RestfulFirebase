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

        public override bool SetBlob(string blob, string tag = null)
        {
            if (base.SetBlob(blob, tag))
            {
                OnChanged(nameof(Blob));
                return true;
            }
            return false;
        }

        public override string GetBlob(string defaultValue = null, string tag = null)
        {
            return base.GetBlob(defaultValue, tag);
        }

        public override bool SetValue<T>(T value, string tag = null)
        {
            try
            {
                return base.SetValue(value, tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public override bool SetRawValue(string value, string tag = null)
        {
            try
            {
                return base.SetRawValue(value, tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public override T GetValue<T>(T defaultValue = default, string tag = null)
        {
            try
            {
                return base.GetValue(defaultValue, tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return defaultValue;
        }

        public override string GetRawValue(string defaultValue = default, string tag = null)
        {
            try
            {
                return base.GetRawValue(defaultValue, tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return defaultValue;
        }

        public override T GetAdditional<T>(string key, T defaultValue = default, string tag = null)
        {
            try
            {
                return base.GetAdditional(key, defaultValue, tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return defaultValue;
        }

        public override bool SetAdditional<T>(string key, T value, string tag = null)
        {
            try
            {
                return base.SetAdditional(key, value, tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public override bool DeleteAdditional(string key, string tag = null)
        {
            try
            {
                return base.DeleteAdditional(key, tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        public override bool ClearAdditionals(string tag = null)
        {
            try
            {
                return base.ClearAdditionals(tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
            return false;
        }

        #endregion
    }

    public class ObservableObject<T> : ObservableObject
    {
        #region Properties

        public T Value
        {
            get => GetValue<T>();
            set => SetValue(Value);
        }

        #endregion

        #region Initializers

        public ObservableObject() : base()
        {

        }

        #endregion
    }
}
