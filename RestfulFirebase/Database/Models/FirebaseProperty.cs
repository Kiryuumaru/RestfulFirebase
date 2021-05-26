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

        private RealtimeModelWire modelWire;

        #endregion

        #region Methods

        protected virtual bool SetBlob(string blob, string tag = null)
        {
            bool hasChanges = false;

            if (modelWire?.Subscribed ?? false && tag != UnwiredBlobTag)
            {
                if (modelWire.Wire.SetBlob(blob)) hasChanges = true;
            }
            else
            {
                if (SetObject(blob)) hasChanges = true;
            }

            return hasChanges;
        }

        protected virtual string GetBlob(string defaultValue = null, string tag = null)
        {
            if (modelWire?.Subscribed ?? false && tag != UnwiredBlobTag)
            {
                return modelWire.GetBlob();
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
                    return Serializer.Deserialize<T>(str);
                }
            }
            catch (Exception ex)
            {
                OnError(ex);
                return defaultValue;
            }
        }

        public void Start()
        {
            //if (!modelWire.Wire.Started) modelWire.Wire.Start();
            modelWire?.Subscribe();
        }

        public void Stop()
        {
            modelWire?.Unsubscribe();
        }

        public void Dispose()
        {
            Stop();
        }

        void IRealtimeModelProxy.StartRealtime(RealtimeModelWire modelWire, bool invokeSetFirst)
        {
            this.modelWire = modelWire;
            modelWire.SetOnSubscribed(delegate
            {
                modelWire.SetOnChanges(args =>
                {
                    OnChanged(nameof(Property));
                });

                var blob = GetBlob(UnwiredBlobTag);

                if (invokeSetFirst)
                {
                    this.modelWire.SetBlob(blob);
                }
                else
                {
                    if (blob != GetBlob())
                    {
                        OnChanged(nameof(Property));
                    }
                }
            });
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
