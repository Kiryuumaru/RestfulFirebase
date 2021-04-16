using Newtonsoft.Json;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Converters;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseProperty : DistinctProperty, IRealtimeModel
    {
        #region Properties

        private const string InitTag = "init";
        private const string SyncTag = "sync";
        private const string RevertTag = "revert";
        public const string ModifiedKey = "_m";

        public RealtimeWire RealtimeWire
        {
            get => Holder.GetAttribute<RealtimeWire>(nameof(RealtimeWire), nameof(FirebaseObject)).Value;
            private set => Holder.SetAttribute(nameof(RealtimeWire), nameof(FirebaseObject), value);
        }

        public SmallDateTime Modified
        {
            get => GetAdditional<SmallDateTime>(ModifiedKey);
            set => SetAdditional(ModifiedKey, value);
        }

        #endregion

        #region Initializers

        public static FirebaseProperty CreateFromKey(string key, SmallDateTime? modified = null)
        {
            var prop = new FirebaseProperty(DistinctProperty.CreateFromKey(key));
            if (modified != null) prop.Modified = modified.Value;
            return prop;
        }

        public static FirebaseProperty<T> CreateFromKey<T>(string key, SmallDateTime? modified = null)
        {
            var prop = new FirebaseProperty(DistinctProperty.CreateFromKey(key));
            if (modified != null) prop.Modified = modified.Value;
            return prop.ParseModel<T>();
        }

        public FirebaseProperty(IAttributed attributed)
            : base(attributed)
        {

        }

        #endregion

        #region Methods

        protected virtual SmallDateTime CurrentDateTimeFactory()
        {
            return SmallDateTime.UtcNow;
        }

        public void Delete()
        {
            UpdateBlob(null);
        }

        public void BuildRealtimeWire(FirebaseQuery query)
        {
            RealtimeWire = new RealtimeWire(query,
                () =>
                {
                    var oldDataFactory = BlobFactory;
                    BlobFactory = new BlobFactory(
                        args =>
                        {
                            if (args.Blob == Blob) return false;
                            void put(string blobToPut, string revertBlob)
                            {
                                RealtimeWire.Query.Put(JsonConvert.SerializeObject(blobToPut), null, ex =>
                                {
                                    if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        UpdateBlob(revertBlob, RevertTag);
                                    }
                                    OnError(ex);
                                });
                            }
                            var newBlob = PrimitiveBlob.CreateFromBlob(args.Blob);
                            var newBlobModified = newBlob.GetAdditional<SmallDateTime>(ModifiedKey);
                            var localData = RealtimeWire.Query.App.Database.OfflineDatabase.GetLocalData(RealtimeWire.Path);
                            var syncData = RealtimeWire.Query.App.Database.OfflineDatabase.GetSyncData(RealtimeWire.Path);
                            switch (args.Tag)
                            {
                                case InitTag:
                                    if (newBlobModified <= SmallDateTime.MinValue) break;
                                    if (args.Blob == null)
                                    {
                                        RealtimeWire.Query.App.Database.OfflineDatabase.DeleteLocalData(RealtimeWire.Path);
                                    }
                                    else
                                    {
                                        RealtimeWire.Query.App.Database.OfflineDatabase.SetLocalData(RealtimeWire.Path, newBlob);
                                    }
                                    break;
                                case SyncTag:
                                    if (args.Blob == null)
                                    {
                                        RealtimeWire.Query.App.Database.OfflineDatabase.DeleteSyncData(RealtimeWire.Path);
                                    }
                                    else
                                    {
                                        RealtimeWire.Query.App.Database.OfflineDatabase.SetSyncData(RealtimeWire.Path, newBlob);
                                    }
                                    if (localData.Modified > syncData.Modified) put(Blob, newBlob.Blob);
                                    break;
                                case RevertTag:
                                    if (args.Blob == null)
                                    {
                                        RealtimeWire.Query.App.Database.OfflineDatabase.DeleteLocalData(RealtimeWire.Path);
                                    }
                                    else
                                    {
                                        RealtimeWire.Query.App.Database.OfflineDatabase.SetLocalData(RealtimeWire.Path, newBlob);
                                    }
                                    break;
                                default:
                                    if (args.Blob == null)
                                    {
                                        put(null, Blob);
                                        RealtimeWire.Query.App.Database.OfflineDatabase.DeleteLocalData(RealtimeWire.Path);
                                        return true;
                                    }
                                    else
                                    {
                                        if (newBlobModified >= Modified)
                                        {
                                            put(newBlob.Blob, Blob);
                                            RealtimeWire.Query.App.Database.OfflineDatabase.SetLocalData(RealtimeWire.Path, newBlob);
                                            return true;
                                        }
                                    }
                                    break;
                            }
                            return args.Blob == RealtimeWire.Query.App.Database.OfflineDatabase.GetData(RealtimeWire.Path)?.PrimitiveBlob.Blob;
                        },
                        args => RealtimeWire.Query.App.Database.OfflineDatabase.GetData(RealtimeWire.Path)?.PrimitiveBlob?.Blob ?? null);
                    BlobFactory.Set(oldDataFactory.Get(), InitTag);
                },
                () =>
                {
                    var oldDataFactory = BlobFactory;
                    BlobFactory = null;
                    BlobFactory.Set(oldDataFactory.Get());
                },
                streamObject =>
                {
                    bool hasChanges = false;
                    try
                    {
                        if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                        else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                        else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                        else if (streamObject.Path.Length == 1) hasChanges = UpdateBlob(streamObject.Data, SyncTag);
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
                    return hasChanges;
                });
        }

        public bool ModifyData(string data)
        {
            if (data == null)
            {
                return UpdateBlob(null);
            }
            else
            {
                var newBlob = PrimitiveBlob.CreateFromBlob(Blob);
                newBlob.UpdateData(data);
                newBlob.SetAdditional(ModifiedKey, CurrentDateTimeFactory());
                return UpdateBlob(newBlob.Blob);
            }
        }

        public FirebaseProperty<T> ParseModel<T>()
        {
            return new FirebaseProperty<T>(this);
        }

        #endregion
    }

    public class FirebaseProperty<T> : FirebaseProperty
    {
        public T Value
        {
            get => DataTypeConverter.GetConverter<T>().Decode(GetData());
            set => ModifyData(DataTypeConverter.GetConverter<T>().Encode(value));
        }

        public FirebaseProperty(IAttributed attributed)
            : base(attributed)
        {

        }

        protected override void OnChanged(string propertyName = "")
        {
            base.OnChanged(nameof(Value));
        }
    }
}
