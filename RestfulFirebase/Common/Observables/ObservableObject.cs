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

        public override bool SetNull(string tag = null)
        {
            try
            {
                if (base.SetNull(tag))
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

        public override bool SetValue<T>(T value, Func<T, T, bool> comparator = null, string tag = null)
        {
            try
            {
                if (base.SetValue(value, comparator, tag))
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
