using Newtonsoft.Json;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Conversions;
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

        private const string ModifiedKey = "_m";
        private const string SyncTag = "sync";
        private const string RevertTag = "revert";

        public bool HasRealtimeWire => RealtimeWirePath != null;

        public string RealtimeWirePath
        {
            get => Holder.GetAttribute<string>(nameof(RealtimeWirePath), nameof(FirebaseProperty)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWirePath), nameof(FirebaseProperty), value);
        }

        public IDisposable RealtimeSubscription
        {
            get => Holder.GetAttribute<IDisposable>(nameof(RealtimeSubscription), nameof(FirebaseProperty)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeSubscription), nameof(FirebaseProperty), value);
        }

        public DateTime Modified
        {
            get => GetAdditional<DateTime>(ModifiedKey);
            set => SetAdditional(ModifiedKey, value);
        }

        #endregion

        #region Initializers

        public static new FirebaseProperty CreateFromKey(string key)
        {
            return new FirebaseProperty(DistinctProperty.CreateFromKey(key));
        }

        public static new FirebaseProperty CreateFromKeyAndValue<T>(string key, T value)
        {
            return new FirebaseProperty(DistinctProperty.CreateFromKeyAndValue(key, value));
        }

        public static new FirebaseProperty CreateFromKeyAndBlob(string key, string blob  )
        {
            return new FirebaseProperty(DistinctProperty.CreateFromKeyAndBlob(key, blob));
        }

        public FirebaseProperty(IAttributed attributed)
            : base(attributed)
        {

        }

        public void Dispose()
        {
            RealtimeSubscription?.Dispose();
            var oldDataFactory = BlobFactory;
            BlobFactory = null;
            UpdateBlob(oldDataFactory.Get());
        }

        #endregion

        #region Methods

        protected virtual DateTime CurrentDateTimeFactory()
        {
            return DateTime.UtcNow;
        }

        public void SetRealtime(IFirebaseQuery query, bool invokeSetFirst)
        {
            RealtimeWirePath = query.GetAbsolutePath();
            var oldDataFactory = BlobFactory;
            BlobFactory = new BlobFactory(
                blob =>
                {
                    if (blob.Value == Blob) return;
                    void put(string blobToPut, string revertBlob)
                    {
                        query.Put(JsonConvert.SerializeObject(blobToPut), null, ex =>
                        {
                            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                UpdateBlob(revertBlob, RevertTag);
                            }
                            OnError(ex);
                        });
                    }
                    var newBlob = PrimitiveBlob.CreateFromBlob(blob.Value);
                    if (blob.Tag == RevertTag)
                    {
                        if (blob.Value == null)
                        {
                            query.App.Database.OfflineDatabase.DeleteData(RealtimeWirePath);
                        }
                        else
                        {
                            query.App.Database.OfflineDatabase.SetData(RealtimeWirePath, newBlob);
                        }
                    }
                    else if (blob.Tag == SyncTag)
                    {
                        if (blob.Value == null)
                        {
                            query.App.Database.OfflineDatabase.DeleteData(RealtimeWirePath);
                        }
                        else
                        {
                            var newBlobModified = newBlob.GetAdditional<DateTime>(ModifiedKey);
                            if (newBlobModified >= Modified)
                            {
                                query.App.Database.OfflineDatabase.SetData(RealtimeWirePath, newBlob);
                            }
                            else
                            {
                                put(Blob, newBlob.Blob);
                            }
                        }
                    }
                    else
                    {
                        if (blob.Value == null)
                        {
                            query.Put(null, null, ex => OnError(ex));
                            query.App.Database.OfflineDatabase.DeleteData(RealtimeWirePath);
                        }
                        else
                        {
                            var newBlobModified = newBlob.GetAdditional<DateTime>(ModifiedKey);
                            if (newBlobModified >= Modified)
                            {
                                put(newBlob.Blob, Blob);
                                query.App.Database.OfflineDatabase.SetData(RealtimeWirePath, newBlob);
                            }
                        }
                    }
                },
                () => query.App.Database.OfflineDatabase.GetData(RealtimeWirePath)?.Blob ?? null);
            if (invokeSetFirst)
            {
                Modified = DateTime.MinValue;
                BlobFactory.Set((oldDataFactory.Get(), null));
            }
            RealtimeSubscription = Observable
                .Create<StreamEvent>(observer => new NodeStreamer(observer, query, (s, e) => OnError(e)).Run())
                .Subscribe(streamEvent =>
                {
                    try
                    {
                        if (streamEvent.Path == null) throw new Exception("StreamEvent Key null");
                        else if (streamEvent.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                        else if (streamEvent.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                        else if (streamEvent.Path.Length == 1) UpdateBlob(streamEvent.Data, SyncTag);
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
                });
        }

        public void ModifyData(string data)
        {
            if (data == null)
            {
                UpdateBlob(null);
            }
            else
            {
                var newBlob = PrimitiveBlob.CreateFromBlob(Blob);
                newBlob.UpdateData(data);
                newBlob.SetAdditional(ModifiedKey, CurrentDateTimeFactory());
                UpdateBlob(newBlob.Blob);
            }
        }

        #endregion
    }
}
