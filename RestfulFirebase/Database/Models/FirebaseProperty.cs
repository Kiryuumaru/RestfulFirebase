using Newtonsoft.Json;
using RestfulFirebase.Auth;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Converters;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseProperty : ObservableProperty, IRealtimeModel
    {
        #region Properties

        protected const string InitTag = "init";
        protected const string SyncTag = "sync";
        protected const string RevertTag = "revert";

        private string BlobHolder
        {
            get => Holder.GetAttribute<string>();
            set => Holder.SetAttribute(value);
        }

        public RealtimeWire Wire
        {
            get => Holder.GetAttribute<RealtimeWire>();
            set => Holder.SetAttribute(value);
        }

        public string Key
        {
            get => Holder.GetAttribute<string>();
            set => Holder.SetAttribute(value);
        }


        #endregion

        #region Initializers

        public FirebaseProperty(IAttributed attributed)
            : base(attributed)
        {

        }

        public FirebaseProperty(string key)
            : base()
        {
            Key = key;
        }

        #endregion

        #region Methods

        protected virtual SmallDateTime CurrentDateTimeFactory()
        {
            return SmallDateTime.UtcNow;
        }

        public override bool SetBlob(string blob, string tag = null)
        {
            if (Wire != null)
            {
                void put(string data)
                {
                    //Wire.Put("\"" + data + "\"", error =>
                    Wire.Put(JsonConvert.SerializeObject(data), error =>
                    {
                        if (Wire == null) return;
                        if (error.Exception.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            SetBlob(null, RevertTag);
                        }
                        OnError(error.Exception);
                    });
                }

                var path = Wire.Query.GetAbsolutePath();
                var offline = Wire.Query.App.Database.OfflineDatabase.GetData(path);

                switch (tag)
                {
                    case InitTag:
                        if (offline.Sync == null)
                        {
                            put(blob);
                            offline.Changes = new DataChanges(blob, blob == null ? DataChangesType.None : DataChangesType.Create);
                        }
                        else if (offline.Changes == null || offline.Blob != blob)
                        {
                            put(blob);
                            offline.Changes = new DataChanges(blob, blob == null ? DataChangesType.Delete : DataChangesType.Update);
                        }
                        return false;
                    case RevertTag:
                        offline.Changes = null;
                        break;
                    case SyncTag:
                        if (Wire.IsWritting && Wire.HasPendingWrite) return false;
                        if (offline.Changes == null)
                        {
                            if (blob == null) offline.Delete();
                            else offline.Sync = blob;
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
                            put(blob);
                            offline.Changes = new DataChanges(blob, blob == null ? DataChangesType.None : DataChangesType.Create);
                        }
                        else if (offline.Changes == null || offline.Blob != blob)
                        {
                            put(blob);
                            offline.Changes = new DataChanges(blob, blob == null ? DataChangesType.Delete : DataChangesType.Update);
                        }
                        break;
                }

                if (offline.HasBlobChanges) OnChanged(nameof(Blob));

                return offline.HasBlobChanges;
            }
            else
            {
                var newData = blob;
                var oldData = BlobHolder;

                var hasBlobChanges = newData != oldData;

                if (hasBlobChanges)
                {
                    BlobHolder = blob;
                    OnChanged(nameof(Blob));
                }

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
                return BlobHolder;
            }
        }

        public void MakeRealtime(RealtimeWire wire)
        {
            wire.OnStart += delegate
            {
                Wire = wire;
                if (Wire.InvokeSetFirst) SetBlob(BlobHolder, InitTag);
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
                    else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
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

        public void Delete()
        {
            SetBlob(null);
        }

        public FirebaseObject<T> ParseModel<T>()
        {
            return new FirebaseObject<T>(this);
        }

        #endregion
    }

    public class FirebaseObject<T> : FirebaseProperty
    {
        #region Properties

        public T Value
        {
            get => base.GetValue<T>();
            set => base.SetValue(value);
        }

        #endregion

        #region Initializers

        public FirebaseObject(IAttributed attributed)
            : base(attributed)
        {

        }

        public FirebaseObject(string key)
            : base(key)
        {

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
