using RestfulFirebase.Common.Converters;
using RestfulFirebase.Common.Converters.Additionals;
using RestfulFirebase.Common.Converters.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Common.Observables
{
    public abstract class ObservableProperty : IAttributed, IObservable
    {
        #region Properties

        public AttributeHolder Holder { get; } = new AttributeHolder();

        private SynchronizationContext Context
        {
            get => Holder.GetAttribute<SynchronizationContext>(AsyncOperationManager.SynchronizationContext);
            set => Holder.SetAttribute(value);
        }

        private PropertyChangedEventHandler PropertyChangedHandler
        {
            get => Holder.GetAttribute<PropertyChangedEventHandler>(delegate { });
            set => Holder.SetAttribute(value);
        }

        private EventHandler<ContinueExceptionEventArgs> PropertyErrorHandler
        {
            get => Holder.GetAttribute<EventHandler<ContinueExceptionEventArgs>>(delegate { });
            set => Holder.SetAttribute(value);
        }

        protected List<PropertyHolder> PropertyHolders
        {
            get => Holder.GetAttribute<List<PropertyHolder>>(new List<PropertyHolder>());
            set => Holder.SetAttribute(value);
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

        public ObservableProperty(IAttributed attributed)
        {
            Holder.Inherit(attributed);
        }

        public ObservableProperty()
        {
            Holder.Inherit(null);
        }

        #endregion

        #region Methods

        public virtual void OnChanged(string propertyName = "")
        {
            var propertyHandler = PropertyChangedHandler;
            void invoke()
            {
                propertyHandler(this, new PropertyChangedEventArgs(propertyName));
            }
            if (propertyHandler != null)
            {
                invoke();
                //Context.Post(s => invoke(), null);
            }
        }

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

        public abstract bool SetValue<T>(T value, string tag = null);

        public abstract bool SetNull(string tag = null);

        public abstract bool IsNull(string tag = null);

        public abstract T GetValue<T>(T defaultValue = default, string tag = null);

        #endregion
    }
}
