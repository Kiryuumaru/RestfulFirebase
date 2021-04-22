using Newtonsoft.Json;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Offline;
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

        public const string ModifiedKey = "m";

        public RealtimeWire RealtimeWire { get; protected set; }

        public string Key { get; protected set; }

        public SmallDateTime Modified
        {
            get => GetAdditional<SmallDateTime>(ModifiedKey);
            set => SetAdditional(ModifiedKey, value);
        }

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
                    var oldFactory = BlobFactory;
                    BlobFactory = new BlobFactory(
                        args =>
                        {
                            void put(string blobToPut, string revertBlob)
                            {
                                query.Put(JsonConvert.SerializeObject(blobToPut), null, ex =>
                                {
                                    if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                                    {
                                        SetBlob(revertBlob, RevertTag);
                                    }
                                    OnError(ex);
                                });
                            }

                            var newData = new OfflineData(args.blob);
                            var localData = query.App.Database.OfflineDatabase.GetLocalData(path);
                            var syncData = query.App.Database.OfflineDatabase.GetSyncData(path);

                            switch (args.tag)
                            {
                                case InitTag:
                                    if (newData.Modified <= SmallDateTime.MinValue) return false;
                                    return query.App.Database.OfflineDatabase.SetLocalData(RealtimeWire.Path, newData);
                                case RevertTag:
                                    return query.App.Database.OfflineDatabase.SetLocalData(RealtimeWire.Path, newData);
                                case SyncTag:
                                    if (newData.Blob == null)
                                    {
                                        var sync = query.App.Database.OfflineDatabase.DeleteSyncData(RealtimeWire.Path);
                                        var local = query.App.Database.OfflineDatabase.DeleteLocalData(RealtimeWire.Path);
                                        return sync || local;
                                    }
                                    else
                                    {
                                        return query.App.Database.OfflineDatabase.SetSyncData(RealtimeWire.Path, newData);
                                    }
                                default:
                                    if (newData.Modified >= Modified)
                                    {
                                        put(newData.GetValue() == null ? null : newData.Blob, Blob);
                                        return query.App.Database.OfflineDatabase.SetLocalData(RealtimeWire.Path, newData);
                                    }
                                    return false;
                            }
                        },
                        args =>
                        {
                            var localData = query.App.Database.OfflineDatabase.GetLocalData(path);
                            var syncData = query.App.Database.OfflineDatabase.GetSyncData(path);
                            return localData.Modified > syncData.Modified ? localData.Blob : syncData.Blob;
                        });
                    BlobFactory.Set(oldFactory.Get(), InitTag);
                },
                delegate
                {

                },
                streamObject =>
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

        public bool DeleteValue(string tag = null)
        {
            if (SetValueNull(tag))
            {
                Modified = CurrentDateTimeFactory();
            }
            return false;
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
