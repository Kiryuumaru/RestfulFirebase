using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseProperty : ObservableProperty, IRealtimeModelProxy
    {
        #region Properties

        private const string UnwiredBlobTag = "unwired";

        internal RealtimeInstance RealtimeInstance { get; private set; }

        #endregion

        #region Methods

        protected virtual bool SetBlob(string blob, object parameter = null)
        {
            bool hasChanges = false;

            if (RealtimeInstance != null && parameter?.ToString() != UnwiredBlobTag)
            {
                if (RealtimeInstance.SetBlob(blob)) hasChanges = true;
            }
            else
            {
                if (SetObject(blob)) hasChanges = true;
            }

            return hasChanges;
        }

        protected virtual string GetBlob(string defaultValue = null, object parameter = null)
        {
            if (RealtimeInstance != null && parameter?.ToString() != UnwiredBlobTag)
            {
                return RealtimeInstance.GetBlob();
            }
            else
            {
                var obj = GetObject(defaultValue, parameter);
                if (obj is string strObj)
                {
                    return strObj;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        public override bool SetValue<T>(T value, object parameter = null)
        {
            try
            {
                var json = Serializer.Serialize(value);
                return SetBlob(json, parameter);
            }
            catch (Exception ex)
            {
                OnError(ex);
                return false;
            }
        }

        public override T GetValue<T>(T defaultValue = default, object parameter = null)
        {
            try
            {
                var str = GetBlob(null, parameter);
                if (str == null)
                {
                    return defaultValue;
                }
                else
                {
                    return Serializer.Deserialize<T>(str, defaultValue);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
                return defaultValue;
            }
        }

        public override bool SetNull(object parameter = null)
        {
            if (RealtimeInstance != null && parameter?.ToString() != UnwiredBlobTag)
            {
                return RealtimeInstance.SetNull();
            }
            else
            {
                return base.SetNull(parameter);
            }
        }

        public override bool IsNull(object parameter = null)
        {
            if (RealtimeInstance != null && parameter?.ToString() != UnwiredBlobTag)
            {
                return RealtimeInstance.IsNull();
            }
            else
            {
                return base.IsNull(parameter);
            }
        }

        public virtual void Dispose()
        {
            Unsubscribe();
            RealtimeInstance = null;
        }

        private void Subscribe()
        {
            RealtimeInstance.OnInternalChanges += RealtimeInstance_OnInternalChanges;
            RealtimeInstance.OnInternalError += RealtimeInstance_OnInternalError;
        }

        private void Unsubscribe()
        {
            RealtimeInstance.OnInternalChanges -= RealtimeInstance_OnInternalChanges;
            RealtimeInstance.OnInternalError -= RealtimeInstance_OnInternalError;
        }

        private void RealtimeInstance_OnInternalChanges(object sender, DataChangesEventArgs e)
        {
            OnChanged(nameof(Property));
        }

        private void RealtimeInstance_OnInternalError(object sender, WireErrorEventArgs e)
        {
            OnError(e.Exception);
        }

        void IRealtimeModelProxy.StartRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            if (RealtimeInstance != null)
            {
                Unsubscribe();
                RealtimeInstance = null;
            }

            RealtimeInstance = realtimeInstance;

            Subscribe();

            var blob = GetBlob(null, UnwiredBlobTag);

            if (invokeSetFirst)
            {
                RealtimeInstance.SetBlob(blob);
            }
            else
            {
                if (blob != GetBlob())
                {
                    OnChanged(nameof(Property));
                }
            }
        }

        #endregion
    }

    public class FirebaseProperty<T> : FirebaseProperty
    {
        #region Properties

        public T Value
        {
            get => base.GetValue<T>();
            set => base.SetValue(value);
        }

        #endregion

        #region Methods

        protected override bool SetBlob(string blob, object parameter = null)
        {
            var hasChanges = false;

            if (base.SetBlob(blob, parameter)) hasChanges = true;

            if (hasChanges) OnChanged(nameof(Value));

            return hasChanges;
        }

        #endregion
    }
}
