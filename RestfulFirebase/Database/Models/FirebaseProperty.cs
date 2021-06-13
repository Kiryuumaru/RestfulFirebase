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

        public virtual void AttachRealtime(RealtimeInstance realtimeInstance, bool invokeSetFirst)
        {
            VerifyNotDisposed();

            if (HasAttachedRealtime)
            {
                Unsubscribe();
                RealtimeInstance = null;
            }

            var obj = GetObject();

            if (obj is IRealtimeModel model)
            {
                RealtimeInstance = realtimeInstance;

                model.AttachRealtime(realtimeInstance, invokeSetFirst);

                Subscribe();
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

                RealtimeInstance = realtimeInstance;

                Subscribe();

                if (invokeSetFirst)
                {
                    RealtimeInstance.SetBlob(blob);
                }
                else
                {
                    SetObject(RealtimeInstance.GetBlob());
                }
            }

            OnRealtimeAttached(new RealtimeInstanceEventArgs(realtimeInstance));
        }

        public virtual void DetachRealtime()
        {
            VerifyNotDisposed();

            if (GetObject() is IRealtimeModel model)
            {
                model.DetachRealtime();
            }

            Unsubscribe();
            var args = new RealtimeInstanceEventArgs(RealtimeInstance);
            RealtimeInstance?.Dispose();
            RealtimeInstance = null;

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
            VerifyNotDisposed();

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
            VerifyNotDisposed();

            if (GetObject() is IRealtimeModel)
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

        private void Subscribe()
        {
            VerifyNotDisposed();

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges += RealtimeInstance_DataChanges;
                RealtimeInstance.Error += RealtimeInstance_Error;
            }
        }

        private void Unsubscribe()
        {
            VerifyNotDisposed();

            if (HasAttachedRealtime)
            {
                RealtimeInstance.DataChanges -= RealtimeInstance_DataChanges;
                RealtimeInstance.Error -= RealtimeInstance_Error;
            }
        }

        private void RealtimeInstance_DataChanges(object sender, DataChangesEventArgs e)
        {
            VerifyNotDisposed();

            var path = Utils.UrlSeparate(e.Path);

            if (path.Length == 0)
            {
                if (!(GetObject() is IRealtimeModel))
                {
                    SetObject(RealtimeInstance.GetBlob());
                }
            }
        }

        private void RealtimeInstance_Error(object sender, WireErrorEventArgs e)
        {
            VerifyNotDisposed();

            OnWireError(e);
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

        #region Initializers

        public FirebaseProperty()
        {
            PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Property))
                {
                    OnPropertyChanged(nameof(Value));
                }
            };
        }

        #endregion

        #region Methods



        #endregion
    }
}
