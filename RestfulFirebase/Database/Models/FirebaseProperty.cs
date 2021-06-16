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
            if (IsDisposed)
            {
                return;
            }

            var obj = GetObject();

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
                        SetObject(RealtimeInstance.GetBlob());
                    }
                }
            }

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        public void DetachRealtime()
        {
            if (IsDisposed)
            {
                return;
            }

            if (GetObject() is IRealtimeModel model)
            {
                model.DetachRealtime();
            }

            var args = new RealtimeInstanceEventArgs(RealtimeInstance);

            Unsubscribe();

            OnRealtimeDetached(args);
        }

        public override bool SetValue<T>(T value)
        {
            if (IsDisposed)
            {
                return false;
            }

            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                if (value is IRealtimeModel model)
                {
                    if (HasAttachedRealtime)
                    {
                        model.AttachRealtime(RealtimeInstance, true);
                    }
                }

                return SetObject(value);
            }
            else
            {
                if (!Serializer.CanSerialize<T>()) throw new Exception("Value is not serializable");

                var blob = Serializer.Serialize(value);

                if (SetObject(blob))
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
            if (IsDisposed)
            {
                return defaultValue;
            }

            if (typeof(IRealtimeModel).IsAssignableFrom(typeof(T)))
            {
                return base.GetValue(defaultValue);
            }
            else
            {
                if (!Serializer.CanSerialize<T>()) throw new Exception("Value is not serializable");

                var obj = GetObject();

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
            if (IsDisposed)
            {
                return false;
            }

            if (GetObject() is IRealtimeModel)
            {
                return base.SetNull();
            }
            else
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
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (GetObject() is IDisposable model)
                {
                    DetachRealtime();
                    SetObject(null);
                    model.Dispose();
                }
                else
                {
                    DetachRealtime();
                    SetObject(null);
                }
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
            if (IsDisposed)
            {
                return;
            }

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
            if (IsDisposed)
            {
                return;
            }

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
            if (IsDisposed)
            {
                return;
            }

            var path = Utils.UrlSeparate(e.Path);

            if (path.Length == 0)
            {
                lock (this)
                {
                    if (!(GetObject() is IRealtimeModel))
                    {
                        SetObject(RealtimeInstance.GetBlob());
                    }
                }
            }
        }

        private void RealtimeInstance_Error(object sender, WireErrorEventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            OnWireError(e);
        }

        private void RealtimeInstance_Disposing(object sender, EventArgs e)
        {
            if (IsDisposed)
            {
                return;
            }

            DetachRealtime();
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
