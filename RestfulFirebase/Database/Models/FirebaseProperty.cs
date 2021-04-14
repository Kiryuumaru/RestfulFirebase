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

        public RealtimeWire RealtimeWire
        {
            get => Holder.GetAttribute<RealtimeWire>(nameof(RealtimeWire), nameof(FirebaseObject)).Value;
            private set => Holder.SetAttribute(nameof(RealtimeWire), nameof(FirebaseObject), value);
        }

        public SmallDateTime Modified
        {
            get => GetAdditional<SmallDateTime>(RealtimeWire.ModifiedKey);
            set => SetAdditional(RealtimeWire.ModifiedKey, value);
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

        public static new FirebaseProperty CreateFromKeyAndData(string key, string data)
        {
            return new FirebaseProperty(DistinctProperty.CreateFromKeyAndData(key, data));
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

        public void BuildRealtimeWire(FirebaseQuery query, bool invokeSetFirst)
        {
            RealtimeWire = new RealtimeWire(query,
                () =>
                {
                    var oldDataFactory = BlobFactory;
                    BlobFactory = new BlobFactory(
                        args =>
                        {
                            if (args.Blob == Blob) return false;
                            async void put(string blobToPut, string revertBlob)
                            {
                                await RealtimeWire.Query.Put(JsonConvert.SerializeObject(blobToPut), null, ex =>
                                {
                                    if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        UpdateBlob(revertBlob, RealtimeWire.RevertTag);
                                    }
                                    OnError(ex);
                                });
                            }
                            var newBlob = PrimitiveBlob.CreateFromBlob(args.Blob);
                            switch (args.Tag)
                            {
                                case RealtimeWire.SyncTag:
                                    if (args.Blob == null)
                                    {
                                        RealtimeWire.Query.App.Database.OfflineDatabase.DeleteData(RealtimeWire.Path);
                                        return true;
                                    }
                                    else
                                    {
                                        var newBlobModified = newBlob.GetAdditional<SmallDateTime>(RealtimeWire.ModifiedKey);
                                        if (newBlobModified >= Modified)
                                        {
                                            RealtimeWire.Query.App.Database.OfflineDatabase.SetData(RealtimeWire.Path, newBlob);
                                            return true;
                                        }
                                        else
                                        {
                                            put(Blob, newBlob.Blob);
                                            return false;
                                        }
                                    }
                                case RealtimeWire.RevertTag:
                                    if (args.Blob == null)
                                    {
                                        RealtimeWire.Query.App.Database.OfflineDatabase.DeleteData(RealtimeWire.Path);
                                    }
                                    else
                                    {
                                        RealtimeWire.Query.App.Database.OfflineDatabase.SetData(RealtimeWire.Path, newBlob);
                                    }
                                    return true;
                                default:
                                    if (args.Blob == null)
                                    {
                                        put(null, Blob);
                                        RealtimeWire.Query.App.Database.OfflineDatabase.DeleteData(RealtimeWire.Path);
                                        return true;
                                    }
                                    else
                                    {
                                        var newBlobModified = newBlob.GetAdditional<SmallDateTime>(RealtimeWire.ModifiedKey);
                                        if (newBlobModified >= Modified)
                                        {
                                            put(newBlob.Blob, Blob);
                                            RealtimeWire.Query.App.Database.OfflineDatabase.SetData(RealtimeWire.Path, newBlob);
                                            return true;
                                        }
                                    }
                                    return false;
                            }
                        },
                        args => RealtimeWire.Query.App.Database.OfflineDatabase.GetData(RealtimeWire.Path)?.Blob ?? null);
                    if (invokeSetFirst)
                    {
                        BlobFactory.Set(oldDataFactory.Get());
                    }
                },
                () =>
                {
                    var oldDataFactory = BlobFactory;
                    BlobFactory = null;
                    BlobFactory.Set(oldDataFactory.Get());
                },
                streamObject =>
                {
                    try
                    {
                        if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                        else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                        else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                        else if (streamObject.Path.Length == 1) UpdateBlob(streamObject.Data, RealtimeWire.SyncTag);
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
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
                newBlob.SetAdditional(RealtimeWire.ModifiedKey, CurrentDateTimeFactory());
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
