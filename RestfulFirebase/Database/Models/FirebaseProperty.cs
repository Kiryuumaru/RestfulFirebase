using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Serializers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseProperty : ObservableProperty, IRealtimeModel
    {
        #region Properties

        public bool HasAttachedRealtime { get => RealtimeInstance != null; }

        public event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;
        public event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;

        internal const string UnwiredBlobTag = "unwired";
        internal const string SerializableTag = "serializable";

        internal RealtimeInstance RealtimeInstance { get; private set; }

        #endregion

        #region Methods

        public virtual void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            VerifyNotDisposed();

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
                    OnPropertyChanged(nameof(Property));
                }
            }

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        public virtual void DetachRealtime()
        {
            VerifyNotDisposed();

            Unsubscribe();
            var args = new RealtimeInstanceEventArgs(RealtimeInstance);
            RealtimeInstance = null;
            OnRealtimeDetached(args);
        }

        public async Task<bool> WaitForSynced(TimeSpan timeout)
        {
            VerifyNotDisposed();

            return await RealtimeInstance.WaitForSynced(timeout);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachRealtime();
            }
            base.Dispose(disposing);
        }

        protected void OnRealtimeAttached(RealtimeInstanceEventArgs args)
        {
            SynchronizationContextSend(delegate
            {
                RealtimeAttached?.Invoke(this, args);
            });
        }

        protected void OnRealtimeDetached(RealtimeInstanceEventArgs args)
        {
            SynchronizationContextSend(delegate
            {
                RealtimeDetached?.Invoke(this, args);
            });
        }

        protected virtual bool SetBlob(string blob, object parameter = null)
        {
            VerifyNotDisposed();

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
            VerifyNotDisposed();

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
            VerifyNotDisposed();

            if (parameter?.ToString() == SerializableTag)
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
            else
            {
                return base.SetValue(value, parameter);
            }
        }

        public override T GetValue<T>(T defaultValue = default, object parameter = null)
        {
            VerifyNotDisposed();

            if (parameter?.ToString() == SerializableTag)
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
            else
            {
                return base.GetValue(defaultValue, parameter);
            }
        }

        public override bool SetNull(object parameter = null)
        {
            VerifyNotDisposed();

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
            VerifyNotDisposed();

            if (RealtimeInstance != null && parameter?.ToString() != UnwiredBlobTag)
            {
                return RealtimeInstance.IsNull();
            }
            else
            {
                return base.IsNull(parameter);
            }
        }

        private void Subscribe()
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                RealtimeInstance.DataChanges += RealtimeInstance_DataChanges;
                RealtimeInstance.Error += RealtimeInstance_Error;
            }
        }

        private void Unsubscribe()
        {
            VerifyNotDisposed();

            if (RealtimeInstance != null)
            {
                RealtimeInstance.DataChanges -= RealtimeInstance_DataChanges;
                RealtimeInstance.Error -= RealtimeInstance_Error;
            }
        }

        private void RealtimeInstance_DataChanges(object sender, DataChangesEventArgs e)
        {
            VerifyNotDisposed();

            OnPropertyChanged(nameof(Property));
        }

        private void RealtimeInstance_Error(object sender, WireErrorEventArgs e)
        {
            VerifyNotDisposed();

            OnError(e.Exception);
        }

        #endregion
    }

    public class FirebaseProperty<T> : FirebaseProperty
    {
        #region Properties

        public T Value
        {
            get => base.GetValue<T>(default, SerializableTag);
            set => base.SetValue(value, SerializableTag);
        }

        #endregion

        #region Methods

        protected override bool SetBlob(string blob, object parameter = null)
        {
            VerifyNotDisposed();

            var hasChanges = false;

            if (base.SetBlob(blob, parameter)) hasChanges = true;

            if (hasChanges) OnPropertyChanged(nameof(Value));

            return hasChanges;
        }

        #endregion
    }
}
