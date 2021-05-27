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

        protected virtual bool SetBlob(string blob, string tag = null)
        {
            bool hasChanges = false;

            if (ModelWire != null && tag != UnwiredBlobTag)
            {
                if (ModelWire.RealtimeInstance.SetBlob(blob)) hasChanges = true;
            }
            else
            {
                if (SetObject(blob)) hasChanges = true;
            }

            return hasChanges;
        }

        protected virtual string GetBlob(string defaultValue = null, string tag = null)
        {
            if (ModelWire != null && tag != UnwiredBlobTag)
            {
                return ModelWire.GetBlob();
            }
            else
            {
                var obj = GetObject(defaultValue, tag);
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

        public override bool SetValue<T>(T value, string tag = null)
        {
            try
            {
                var json = Serializer.Serialize(value);
                return SetBlob(json, tag);
            }
            catch (Exception ex)
            {
                OnError(ex);
                return false;
            }
        }

        public override T GetValue<T>(T defaultValue = default, string tag = null)
        {
            try
            {
                var str = GetBlob(null, tag);
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

        public void Dispose()
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

        protected override bool SetBlob(string blob, string tag = null)
        {
            var hasChanges = false;

            if (base.SetBlob(blob, tag)) hasChanges = true;

            if (hasChanges) OnChanged(nameof(Value));

            return hasChanges;
        }

        #endregion
    }
}
