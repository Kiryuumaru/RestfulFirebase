using Newtonsoft.Json;
using RestfulFirebase.Auth;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Database.Realtime;
using ObservableHelpers.Observables;

namespace RestfulFirebase.Database.Models.Primitive
{
    public class FirebaseProperty : ObservableSerializableProperty, IRealtimeModel
    {
        #region Properties

        protected const string InitTag = "init";
        protected const string SyncTag = "sync";

        public RealtimeWire Wire { get; private set; }
        public DataNode Node { get; private set; }

        #endregion

        #region Methods

        public override bool SetBlob(string blob, string tag = null)
        {
            bool hasChanges = false;

            void onError(RetryExceptionEventArgs err)
            {
                if (err.Exception is FirebaseDatabaseException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        if (Node.DeleteChanges()) OnChanged(nameof(Blob));
                    }
                }
                OnError(err.Exception);
            }

            if (Wire != null)
            {
                switch (tag)
                {
                    case InitTag:
                        if (Wire.InvokeSetFirst)
                        {
                            if (Node.MakeChanges(blob, onError)) hasChanges = true;
                            return hasChanges;
                        }
                        else
                        {
                            if (blob != Node.Changes?.Blob) hasChanges = true;
                        }
                        break;
                    case SyncTag:
                        if (Node.MakeSync(blob, onError)) hasChanges = true;
                        break;
                    default:
                        if (Node.MakeChanges(blob, onError)) hasChanges = true;
                        break;
                }
            }
            else
            {
                if (base.SetBlob(blob)) hasChanges = true;
            }

            if (hasChanges) OnChanged(nameof(Blob));

            return hasChanges;
        }

        public override string GetBlob(string defaultValue = null, string tag = null)
        {
            if (Wire != null)
            {
                return Node.Blob;
            }
            else
            {
                return base.GetBlob(defaultValue, tag);
            }
        }

        public void MakeRealtime(RealtimeWire wire)
        {
            wire.OnStart += delegate
            {
                Wire = wire;
                Node = new DataNode(wire);
                SetBlob(base.GetBlob(), InitTag);
            };
            wire.OnStop += delegate
            {
                Wire = null;
                Node = null;
            };
            wire.OnStream += streamObject =>
            {
                bool hasChanges = false;
                try
                {
                    if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                    else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                    else if (streamObject.Path.Length == 1)
                    {
                        if (streamObject.Object is SingleStreamData obj)
                        {
                            hasChanges = SetBlob(obj.Data, SyncTag);
                        }
                        else if (streamObject.Object is null)
                        {
                            hasChanges = SetBlob(null, SyncTag);
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
                return hasChanges;
            };
        }

        public bool Delete()
        {
            return SetBlob(null);
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

        public override bool SetBlob(string blob, string tag = null)
        {
            var hasChanges = base.SetBlob(blob, tag);
            if (hasChanges) OnChanged(nameof(Value));
            return hasChanges;
        }

        #endregion
    }
}
