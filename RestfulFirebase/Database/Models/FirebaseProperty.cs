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

        private const string ModifiedKey = "_m";
        private const string RealtimeInitializeTag = "realtime_init";
        private const string SyncTag = "sync";
        private const string RevertTag = "revert";

        public bool HasRealtimeWire => RealtimeWire != null;

        public string RealtimeWirePath => RealtimeWire.GetAbsolutePath();

        public FirebaseQuery RealtimeWire
        {
            get => Holder.GetAttribute<FirebaseQuery>(nameof(RealtimeWire), nameof(FirebaseProperty)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWire), nameof(FirebaseProperty), value);
        }

        public SmallDateTime Modified
        {
            get => GetAdditional<SmallDateTime>(ModifiedKey);
            set => SetAdditional(ModifiedKey, value);
        }

        #endregion

        #region Initializers

        public static new FirebaseProperty CreateFromKey(string key)
        {
            return new FirebaseProperty(DistinctProperty.CreateFromKey(key));
        }

        public static new FirebaseProperty<T> CreateFromKeyAndValue<T>(string key, T value)
        {
            return new FirebaseProperty<T>(DistinctProperty.CreateFromKeyAndValue(key, value));
        }

        public static new FirebaseProperty CreateFromKeyAndBlob(string key, string blob)
        {
            return new FirebaseProperty(DistinctProperty.CreateFromKeyAndBlob(key, blob));
        }

        public FirebaseProperty(IAttributed attributed)
            : base(attributed)
        {

        }

        #endregion

        #region Methods

        protected virtual SmallDateTime CurrentDateTimeFactory()
        {
            return new SmallDateTime(DateTime.UtcNow);
        }

        public void Delete()
        {
            UpdateBlob(null);
        }

        public void StartRealtime(FirebaseQuery query, bool invokeSetFirst, out Action<StreamObject> onNext)
        {
            RealtimeWire = query;
            var oldDataFactory = BlobFactory;
            BlobFactory = new BlobFactory(
                blob =>
                {
                    if (blob.Value == Blob) return false;
                    async void put(string blobToPut, string revertBlob)
                    {
                        await query.Put(JsonConvert.SerializeObject(blobToPut), null, ex =>
                        {
                            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                            {
                                UpdateBlob(revertBlob, RevertTag);
                            }
                            OnError(ex);
                        });
                    }
                    var newBlob = PrimitiveBlob.CreateFromBlob(blob.Value);
                    switch (blob.Tag)
                    {
                        case SyncTag:
                            if (blob.Value == null)
                            {
                                query.App.Database.OfflineDatabase.DeleteData(RealtimeWirePath);
                                return true;
                            }
                            else
                            {
                                var newBlobModified = newBlob.GetAdditional<SmallDateTime>(ModifiedKey);
                                if (newBlobModified >= Modified)
                                {
                                    query.App.Database.OfflineDatabase.SetData(RealtimeWirePath, newBlob);
                                    return true;
                                }
                                else
                                {
                                    put(Blob, newBlob.Blob);
                                    return false;
                                }
                            }
                        case RevertTag:
                            if (blob.Value == null)
                            {
                                query.App.Database.OfflineDatabase.DeleteData(RealtimeWirePath);
                            }
                            else
                            {
                                query.App.Database.OfflineDatabase.SetData(RealtimeWirePath, newBlob);
                            }
                            return true;
                        default:
                            if (blob.Value == null)
                            {
                                put(null, Blob);
                                query.App.Database.OfflineDatabase.DeleteData(RealtimeWirePath);
                                return true;
                            }
                            else
                            {
                                var newBlobModified = newBlob.GetAdditional<SmallDateTime>(ModifiedKey);
                                if (newBlobModified >= Modified)
                                {
                                    put(newBlob.Blob, Blob);
                                    query.App.Database.OfflineDatabase.SetData(RealtimeWirePath, newBlob);
                                    return true;
                                }
                            }
                            return false;
                    }
                },
                () => query.App.Database.OfflineDatabase.GetData(RealtimeWirePath)?.Blob ?? null);
            if (invokeSetFirst)
            {
                BlobFactory.Set((oldDataFactory.Get(), null));
            }
            onNext = new Action<StreamObject>(streamObject =>
            {
                if (!HasRealtimeWire) throw new Exception("Model is not realtime");
                try
                {
                    if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                    else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                    else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                    else if (streamObject.Path.Length == 1) UpdateBlob(streamObject.Data, SyncTag);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
            });
        }

        public void ConsumeStream(StreamEvent streamEvent)
        {
            if (!HasRealtimeWire) throw new Exception("Model is not realtime");
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
