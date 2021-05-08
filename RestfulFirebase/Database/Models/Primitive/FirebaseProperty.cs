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
        protected const string RevertTag = "revert";

        public RealtimeWire Wire { get; private set; }

        #endregion

        #region Methods

        public override bool SetBlob(string blob, string tag = null)
        {
            if (Wire != null)
            {
                void put(string data)
                {
                    Wire.Put(JsonConvert.SerializeObject(data), error =>
                    {
                        if (Wire == null) return;
                        if (error.Exception is FirebaseDatabaseException ex)
                        {
                            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                SetBlob(null, RevertTag);
                            }
                        }
                        OnError(error.Exception);
                    });
                }

                var path = Wire.Query.GetAbsolutePath();
                var offline = Wire.Query.App.Database.OfflineDatabase.GetData(path);

                switch (tag)
                {
                    case InitTag:
                        if (Wire.InvokeSetFirst)
                        {
                            if (offline.Sync == null)
                            {
                                offline.Changes = new DataChanges(blob, blob == null ? DataChangesType.None : DataChangesType.Create);
                                put(blob);
                            }
                            else if (offline.Changes == null || offline.Blob != blob)
                            {
                                offline.Changes = new DataChanges(blob, blob == null ? DataChangesType.Delete : DataChangesType.Update);
                                put(blob);
                            }
                        }
                        else
                        {
                            var hasChanges = blob != offline.Changes?.Blob;
                            if (hasChanges) OnChanged(nameof(Blob));
                            return hasChanges;
                        }
                        return false;
                    case RevertTag:
                        offline.Changes = null;
                        break;
                    case SyncTag:
                        if (offline.Changes == null)
                        {
                            if (blob == null) offline.Delete();
                            else offline.Sync = blob;
                        }
                        else if (offline.Changes.Blob == blob)
                        {
                            offline.Sync = blob;
                            offline.Changes = null;
                        }
                        else
                        {
                            switch (offline.Changes.ChangesType)
                            {
                                case DataChangesType.Create:
                                    if (blob == null)
                                    {
                                        put(offline.Changes.Blob);
                                        return false;
                                    }
                                    else
                                    {
                                        offline.Sync = blob;
                                        offline.Changes = null;
                                        break;
                                    }
                                case DataChangesType.Update:
                                    if (blob == null)
                                    {
                                        offline.Delete();
                                        break;
                                    }
                                    else if (offline.Sync == blob)
                                    {
                                        put(offline.Changes.Blob);
                                        return false;
                                    }
                                    else
                                    {
                                        offline.Sync = blob;
                                        offline.Changes = null;
                                        break;
                                    }
                                case DataChangesType.Delete:
                                    if (blob == null)
                                    {
                                        return false;
                                    }
                                    else if (offline.Sync == blob)
                                    {
                                        put(null);
                                        return false;
                                    }
                                    else
                                    {
                                        offline.Sync = blob;
                                        offline.Changes = null;
                                        break;
                                    }
                                case DataChangesType.None:
                                    offline.Sync = blob;
                                    offline.Changes = null;
                                    break;
                            }
                        }
                        break;
                    default:
                        if (offline.Sync == null)
                        {
                            offline.Changes = new DataChanges(blob, blob == null ? DataChangesType.None : DataChangesType.Create);
                            put(blob);
                        }
                        else if (offline.Changes == null || offline.Blob != blob)
                        {
                            offline.Changes = new DataChanges(blob, blob == null ? DataChangesType.Delete : DataChangesType.Update);
                            put(blob);
                        }
                        break;
                }

                if (offline.HasBlobChanges) OnChanged(nameof(Blob));

                return offline.HasBlobChanges;
            }
            else
            {
                var hasBlobChanges = base.SetBlob(blob);

                if (hasBlobChanges) OnChanged(nameof(Blob));

                return hasBlobChanges;
            }
        }

        public override string GetBlob(string defaultValue = null, string tag = null)
        {
            if (Wire != null)
            {
                var path = Wire.Query.GetAbsolutePath();
                return Wire.Query.App.Database.OfflineDatabase.GetData(path).Blob;
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
                SetBlob(base.GetBlob(), InitTag);
            };
            wire.OnStop += delegate
            {
                Wire = null;
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
