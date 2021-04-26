using Newtonsoft.Json;
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
    public class FirebaseObject : ObservableObject, IRealtimeModel
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

        public FirebaseObject(IAttributed attributed)
            : base(attributed)
        {

        }

        public FirebaseObject(string key)
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
                    Wire.Put(JsonConvert.SerializeObject(data), ex =>
                    {
                        if (Wire == null) return;
                        if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            SetBlob(null, RevertTag);
                        }
                        OnError(ex);
                    });
                }

                var path = Wire.Query.GetAbsolutePath();
                var offline = Wire.Query.App.Database.OfflineDatabase.GetData(path);

                switch (tag)
                {
                    case InitTag:
                        if (blob == null) return false;
                        if (offline.SyncBlob == null)
                        {
                            put(blob);
                            offline.Changes = new OfflineChanges(blob, blob == null ? OfflineChangesType.None : OfflineChangesType.Create);
                        }
                        else if (offline.Changes == null || offline.LatestBlob != blob)
                        {
                            put(blob);
                            offline.Changes = new OfflineChanges(blob, blob == null ? OfflineChangesType.Delete : OfflineChangesType.Update);
                        }
                        return false;
                    case RevertTag:
                        offline.Changes = null;
                        break;
                    case SyncTag:
                        if (offline.Changes == null)
                        {
                            if (blob == null) offline.Delete();
                            else offline.SyncBlob = blob;
                        }
                        else
                        {
                            switch (offline.Changes.ChangesType)
                            {
                                case OfflineChangesType.Create:
                                    if (blob == null)
                                    {
                                        put(offline.Changes.Blob);
                                        return false;
                                    }
                                    else
                                    {
                                        offline.SyncBlob = blob;
                                        offline.Changes = null;
                                        break;
                                    }
                                case OfflineChangesType.Update:
                                    if (blob == null)
                                    {
                                        offline.Delete();
                                        break;
                                    }
                                    else if (offline.SyncBlob == blob)
                                    {
                                        put(offline.Changes.Blob);
                                        return false;
                                    }
                                    else
                                    {
                                        offline.SyncBlob = blob;
                                        offline.Changes = null;
                                        break;
                                    }
                                case OfflineChangesType.Delete:
                                    if (blob == null)
                                    {
                                        return false;
                                    }
                                    else if (offline.SyncBlob == blob)
                                    {
                                        put(null);
                                        return false;
                                    }
                                    else
                                    {
                                        offline.SyncBlob = blob;
                                        offline.Changes = null;
                                        break;
                                    }
                                case OfflineChangesType.None:
                                    offline.SyncBlob = blob;
                                    offline.Changes = null;
                                    break;
                            }
                        }
                        break;
                    default:
                        if (offline.SyncBlob == null)
                        {
                            put(blob);
                            offline.Changes = new OfflineChanges(blob, blob == null ? OfflineChangesType.None : OfflineChangesType.Create);
                        }
                        else if (offline.Changes == null || offline.LatestBlob != blob)
                        {
                            put(blob);
                            offline.Changes = new OfflineChanges(blob, blob == null ? OfflineChangesType.Delete : OfflineChangesType.Update);
                        }
                        break;
                }

                if (offline.HasLatestBlobChanges) OnChanged(nameof(Blob));

                return offline.HasLatestBlobChanges;
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
                return Wire.Query.App.Database.OfflineDatabase.GetData(path).LatestBlob;
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
                SetBlob(BlobHolder, InitTag);
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
                    else if (streamObject.Path.Length == 1) hasChanges = SetBlob(streamObject.Data, SyncTag);
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

    public class FirebaseObject<T> : FirebaseObject
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
