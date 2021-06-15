using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Extensions;
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

        public RealtimeInstance RealtimeInstance { get; private set; }

        public bool HasAttachedRealtime { get => !(RealtimeInstance?.IsDisposed ?? true); }

        public event EventHandler<RealtimeInstanceEventArgs> RealtimeAttached;
        public event EventHandler<RealtimeInstanceEventArgs> RealtimeDetached;
        public event EventHandler<WireErrorEventArgs> WireError;

        #endregion

        #region Methods

        public void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            VerifyNotDisposed();

            var obj = GetObjectCore();

            lock (this)
            {
                Subscribe(realtimeInstance);

                if (obj is IRealtimeModel model)
                {
                    model.AttachRealtime(realtimeInstance, invokeSetFirst);
                }
                else
                {
                    string blob = null;
                    if (obj is string objBlob)
                    {
                        blob = objBlob;
                    }
                    else if (obj is null)
                    {
                        blob = null;
                    }
                    else
                    {
                        throw new Exception("Object is not serializable");
                    }

                    if (invokeSetFirst)
                    {
                        RealtimeInstance.SetBlob(blob);
                    }
                    else
                    {
                        SetObjectCore(RealtimeInstance.GetBlob());
                    }
                }
            }

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        public void DetachRealtime()
        {
            VerifyNotDisposed();

            if (GetObjectCore() is IRealtimeModel model)
            {
                model.DetachRealtime();
            }

            var args = new RealtimeInstanceEventArgs(RealtimeInstance);

            Unsubscribe();

            OnRealtimeDetached(args);
        }

        public override bool SetValue<T>(T value)
        {
            VerifyNotDisposed();

            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                if (value is IRealtimeModel model)
                {
                    if (HasAttachedRealtime)
                    {
                        model.AttachRealtime(RealtimeInstance, true);
                    }
                }

                return SetObjectCore(value);
            }
            else
            {
                if (!Serializer.CanSerialize<T>()) throw new Exception("Value is not serializable");

                var blob = Serializer.Serialize(value);

                if (SetObjectCore(blob))
                {
                    if (HasAttachedRealtime)
                    {
                        RealtimeInstance.SetBlob(blob);
                    }

                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public override T GetValue<T>(T defaultValue = default)
        {
            VerifyNotDisposed();

            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                return base.GetValue(defaultValue);
            }
            else
            {
                if (!Serializer.CanSerialize<T>()) throw new Exception("Value is not serializable");

                var obj = GetObjectCore();

                string blob = null;
                if (obj is string objBlob)
                {
                    blob = objBlob;
                }
                else if (obj is null)
                {
                    blob = null;
                }
                else
                {
                    throw new Exception("Object is not serializable");
                }

                return Serializer.Deserialize<T>(blob, defaultValue);
            }
        }

        public override bool SetNull()
        {
            VerifyNotDisposed();

            if (GetObjectCore() is IRealtimeModel)
            {
                if (base.SetNull())
                {
                    if (HasAttachedRealtime)
                    {
                        RealtimeInstance.SetBlob(null);
                    }
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return base.SetNull();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DetachRealtime();
                if (GetObjectCore() is IDisposable model)
                {
                    model.Dispose();
                }
                SetObjectCore(null);
            }
            base.Dispose(disposing);
        }

        protected virtual void OnRealtimeAttached(RealtimeInstanceEventArgs args)
        {
            ContextPost(delegate
            {
                RealtimeAttached?.Invoke(this, args);
            });
        }

        protected virtual void OnRealtimeDetached(RealtimeInstanceEventArgs args)
        {
            ContextPost(delegate
            {
                RealtimeDetached?.Invoke(this, args);
            });
        }

        protected virtual void OnWireError(WireErrorEventArgs args)
        {
            ContextPost(delegate
            {
                WireError?.Invoke(this, args);
            });
        }

        private void Subscribe(RealtimeInstance realtimeInstance)
        {
            VerifyNotDisposed();

            if (HasAttachedRealtime)
            {
                Unsubscribe();
            }

            RealtimeInstance = realtimeInstance;

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges += RealtimeInstance_DataChanges;
                RealtimeInstance.Error += RealtimeInstance_Error;
                RealtimeInstance.Disposing += RealtimeInstance_Disposing;
            }
        }

        private void Unsubscribe()
        {
            VerifyNotDisposed();

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges -= RealtimeInstance_DataChanges;
                RealtimeInstance.Error -= RealtimeInstance_Error;
                RealtimeInstance.Disposing -= RealtimeInstance_Disposing;
            }

            RealtimeInstance = null;
        }

        private void RealtimeInstance_DataChanges(object sender, DataChangesEventArgs e)
        {
            VerifyNotDisposed();

            var path = Utils.UrlSeparate(e.Path);

            if (path.Length == 0)
            {
                lock (this)
                {
                    if (!(GetObjectCore() is IRealtimeModel))
                    {
                        SetObjectCore(RealtimeInstance.GetBlob());
                    }
                }
            }
        }

        private void RealtimeInstance_Error(object sender, WireErrorEventArgs e)
        {
            VerifyNotDisposed();

            OnWireError(e);
        }

        private void RealtimeInstance_Disposing(object sender, EventArgs e)
        {
            Dispose();
        }

        #endregion
    }

    public class FirebaseProperty<T> : FirebaseProperty
    {
        #region Properties

        public T Value
        {
            get => base.GetValue<T>(default);
            set => base.SetValue(value);
        }

        #endregion

        #region Methods

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(Property))
            {
                base.OnPropertyChanged(nameof(Value));
            }
        }

        #endregion
    }
}
