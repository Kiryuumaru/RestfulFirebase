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

        internal RealtimeModelWire ModelWire { get; private set; }

        #endregion

        #region Methods

        protected virtual bool SetBlob(string blob, object parameter = null)
        {
            bool hasChanges = false;

            if (ModelWire != null && parameter?.ToString() != UnwiredBlobTag)
            {
                if (ModelWire.RealtimeInstance.SetBlob(blob)) hasChanges = true;
            }
            else
            {
                if (SetObject(blob)) hasChanges = true;
            }

            return hasChanges;
        }

        protected virtual string GetBlob(string defaultValue = null, object parameter = null)
        {
            if (ModelWire != null && parameter?.ToString() != UnwiredBlobTag)
            {
                return ModelWire.GetBlob();
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
            if (ModelWire != null && parameter?.ToString() != UnwiredBlobTag)
            {
                if (!IsNull(parameter)) return ModelWire.SetBlob(null);
                return false;
            }
            else
            {
                if (!IsNull(parameter)) return base.SetNull(parameter);
                return false;
            }
        }

        public override bool IsNull(object parameter = null)
        {
            if (ModelWire != null && parameter?.ToString() != UnwiredBlobTag)
            {
                return ModelWire.GetBlob() == null;
            }
            else
            {
                return base.IsNull(parameter);
            }
        }

        public virtual void Dispose()
        {
            ModelWire?.Unsubscribe();
            ModelWire = null;
        }

        void IRealtimeModelProxy.StartRealtime(RealtimeModelWire modelWire, bool invokeSetFirst)
        {
            if (ModelWire != null)
            {
                ModelWire?.Unsubscribe();
                ModelWire = null;
            }

            ModelWire = modelWire;

            ModelWire.Subscribe();

            ModelWire.SetOnChanges(args =>
            {
                OnChanged(nameof(Property));
            });

            var blob = GetBlob(null, UnwiredBlobTag);

            if (invokeSetFirst)
            {
                ModelWire.SetBlob(blob);
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
