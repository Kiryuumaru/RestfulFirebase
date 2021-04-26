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

        private string LastPush
        {
            get => Holder.GetAttribute<string>();
            set => Holder.SetAttribute(value);
        }

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
                    if (LastPush == data) return;
                    LastPush = data;
                    Wire.Put(JsonConvert.SerializeObject(LastPush), ex =>
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
                var local = Wire.Query.App.Database.OfflineDatabase.GetData(path);

                switch (tag)
                {
                    case InitTag:
                        if (blob == null) return false;
                        local.LocalBlob = blob;
                        return false;
                    case RevertTag:
                        local.LocalBlob = blob;
                        break;
                    case SyncTag:
                        if (blob == null)
                        {
                            if (local.SyncBlob != null)
                            {
                                local.Delete();
                            }
                            else if (local.LocalBlob != null)
                            {
                                put(local.LocalBlob);
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (local.LocalBlob == null)
                            {
                                local.SyncBlob = blob;
                            }
                            else if (blob == local.SyncBlob && blob != local.LocalBlob)
                            {
                                put(local.LocalBlob);
                                return false;
                            }
                            else
                            {
                                local.SyncBlob = blob;
                                local.LocalBlob = null;
                            }
                        }
                        break;
                    default:
                        if (local.CurrentBlob != blob)
                        {
                            put(blob);
                            local.LocalBlob = blob;
                        }
                        break;
                }

                if (local.HasCurrentBlobChanges) OnChanged(nameof(Blob));

                return local.HasCurrentBlobChanges;
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
                return Wire.Query.App.Database.OfflineDatabase.GetData(path).CurrentBlob;
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
