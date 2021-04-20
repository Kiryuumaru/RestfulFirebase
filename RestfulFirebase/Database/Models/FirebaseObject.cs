using Newtonsoft.Json;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObject : ObservableObject, IRealtimeModel
    {
        #region Properties

        private const string InitTag = "init";
        private const string SyncTag = "sync";
        private const string RevertTag = "revert";

        public string Key { get; protected set; }

        public SmallDateTime Modified { get; protected set; }

        public RealtimeWire RealtimeWire { get; protected set; }

        #endregion

        #region Initializers

        public FirebaseObject(string key) : base()
        {
            Key = key;
        }

        #endregion

        #region Methods

        protected virtual SmallDateTime CurrentDateTimeFactory()
        {
            return SmallDateTime.UtcNow;
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public void BuildRealtimeWire(FirebaseQuery parent)
        {
            var path = parent.GetAbsolutePath();
            var query = new ChildQuery(parent.App, parent, () => path);
            RealtimeWire = new RealtimeWire(query,
                delegate
                {
                    BlobFactory = new BlobFactory(
                        args =>
                        {
                            void put(string blobToPut, string revertBlob)
                            {
                                query.Put(JsonConvert.SerializeObject(blobToPut), null, ex =>
                                {
                                    if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        SetValue(revertBlob, RevertTag);
                                    }
                                    OnError(ex);
                                });
                            }

                            var newData = args.blob;
                            var localData = query.App.Database.OfflineDatabase.GetLocalData(path);
                            var syncData = query.App.Database.OfflineDatabase.GetSyncData(path);

                            switch (args.tag)
                            {
                                case InitTag:
                                    if (newData.Modified <= SmallDateTime.MinValue) break;
                                    if (args.Blob == null)
                                    {
                                        query.App.Database.OfflineDatabase.DeleteLocalData(RealtimeWire.Path);
                                    }
                                    else
                                    {
                                        query.App.Database.OfflineDatabase.SetLocalData(RealtimeWire.Path, newData);
                                    }
                                    break;
                                case RevertTag:
                                    if (args.Blob == null)
                                    {
                                        query.App.Database.OfflineDatabase.DeleteLocalData(RealtimeWire.Path);
                                    }
                                    else
                                    {
                                        query.App.Database.OfflineDatabase.SetLocalData(RealtimeWire.Path, newData);
                                    }
                                    break;
                                case SyncTag:
                                    if (args.Blob == null)
                                    {
                                        var localData = query.App.Database.OfflineDatabase.GetLocalData(RealtimeWire.Path);
                                        var syncData = query.App.Database.OfflineDatabase.GetSyncData(RealtimeWire.Path);
                                        query.App.Database.OfflineDatabase.DeleteSyncData(RealtimeWire.Path);
                                        if (syncData != null)
                                        {
                                            query.App.Database.OfflineDatabase.DeleteLocalData(RealtimeWire.Path);
                                        }
                                        else if (lastData != null)
                                        {
                                            if (lastData.Modified > newData.Modified) put(lastData.PrimitiveBlob.Blob, Blob);
                                        }
                                    }
                                    else
                                    {
                                        query.App.Database.OfflineDatabase.SetSyncData(RealtimeWire.Path, newData);
                                        if (lastData != null) if (lastData.Modified > newData.Modified) put(lastData.PrimitiveBlob.Blob, Blob);
                                    }
                                    break;
                                default:
                                    if (args.Blob == null)
                                    {
                                        put(null, Blob);
                                        var localLast = query.App.Database.OfflineDatabase.GetLocalData(RealtimeWire.Path);
                                        query.App.Database.OfflineDatabase.DeleteLocalData(RealtimeWire.Path);
                                        var localCurrent = query.App.Database.OfflineDatabase.GetLocalData(RealtimeWire.Path);
                                        if (localLast == null && localCurrent == null) return false;
                                        return localLast?.PrimitiveBlob.Blob != localCurrent?.PrimitiveBlob.Blob;
                                    }
                                    else
                                    {
                                        if (newData.Modified >= Modified)
                                        {
                                            put(newData.PrimitiveBlob.Blob, Blob);
                                            query.App.Database.OfflineDatabase.SetLocalData(RealtimeWire.Path, newData);
                                        }
                                    }
                                    break;
                            }



                            var current = query.App.Database.OfflineDatabase.GetData(RealtimeWire.Path);
                            if (lastData == null && current == null) return false;
                            return lastData?.PrimitiveBlob.Blob != current?.PrimitiveBlob.Blob;
                        },
                        args =>
                        {
                            var type = typeof(args.defaultValue);
                            var localData = query.App.Database.OfflineDatabase.GetLocalData(path);
                            var syncData = query.App.Database.OfflineDatabase.GetSyncData(path);
                            var localDataModified = Helpers.Des
        
                        });
                },
                delegate
                {

                },
                streamObject =>
                {

                });
        }

        public bool ModifyValue<T>(T value, string tag = null)
        {
            if (SetValue(value, tag))
            {
                Modified = CurrentDateTimeFactory();
            }
            return false;
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
            set => base.ModifyValue(value);
        }

        #endregion

        #region Initializers

        public FirebaseObject(string key)
            : base(key)
        {

        }

        #endregion

        #region Methods



        #endregion
    }
}
