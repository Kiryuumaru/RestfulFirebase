using Newtonsoft.Json;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Converters;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObject : ObservableObject, IRealtimeModel
    {
        #region Properties

        private string blobHolder = null;

        protected const string InitTag = "init";
        protected const string SyncTag = "sync";
        protected const string RevertTag = "revert";

        public const string ModifiedKey = "m";

        public FirebaseQuery Query { get; private set; }
        public bool HasFirstStream { get; private set; }
        public string Key { get; protected set; }

        public SmallDateTime Modified
        {
            get => GetAdditional<SmallDateTime>(ModifiedKey);
            set => SetAdditional(ModifiedKey, value);
        }

        #endregion

        #region Initializers

        public FirebaseObject(string key)
            : base()
        {
            Key = key;
            Modified = SmallDateTime.MinValue;
        }

        #endregion

        #region Methods

        protected virtual SmallDateTime CurrentDateTimeFactory()
        {
            return SmallDateTime.UtcNow;
        }

        public override bool SetBlob(string blob, string tag = null)
        {
            if (Query != null)
            {
                var path = Query.GetAbsolutePath();

                void put(string blobToPut, string revertBlob)
                {
                    Query.Put(JsonConvert.SerializeObject(blobToPut), null, ex =>
                    {
                        if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            SetBlob(revertBlob, RevertTag);
                        }
                        OnError(ex);
                    });
                }

                var newData = new OfflineData(blob);
                var localData = Query.App.Database.OfflineDatabase.GetLocalData(path);
                var syncData = Query.App.Database.OfflineDatabase.GetSyncData(path);
                var currData = localData.Modified > syncData.Modified ? localData : syncData;

                switch (tag)
                {
                    case InitTag:
                        if (newData.Modified <= SmallDateTime.MinValue) return false;
                        Query.App.Database.OfflineDatabase.SetLocalData(path, newData);
                        break;
                    case RevertTag:
                        Query.App.Database.OfflineDatabase.SetLocalData(path, newData);
                        break;
                    case SyncTag:
                        if (newData.Blob == null)
                        {
                            Query.App.Database.OfflineDatabase.DeleteSyncData(path);
                            Query.App.Database.OfflineDatabase.DeleteLocalData(path);
                        }
                        else
                        {
                            Query.App.Database.OfflineDatabase.SetSyncData(path, newData);
                        }
                        break;
                    default:
                        if (newData.Modified >= Modified)
                        {
                            put(newData.GetRawValue() == null ? null : newData.Blob, Blob);
                            Query.App.Database.OfflineDatabase.SetLocalData(path, newData);
                        }
                        break;
                }

                var newLocalData = Query.App.Database.OfflineDatabase.GetLocalData(path);
                var newSyncData = Query.App.Database.OfflineDatabase.GetSyncData(path);
                var newCurrData = newLocalData.Modified > newSyncData.Modified ? newLocalData : newSyncData;

                var hasBlobChanges = newCurrData.Blob != currData.Blob;
                var hasModifiedChanges = newCurrData.Modified != currData.Modified;

                if (hasBlobChanges) OnChanged(nameof(Blob));
                if (hasModifiedChanges) OnChanged(nameof(Modified));

                return hasBlobChanges || hasModifiedChanges;
            }
            else
            {
                var newData = new OfflineData(blob);
                var oldData = new OfflineData(blobHolder);

                blobHolder = blob;

                var hasBlobChanges = newData.Blob != oldData.Blob;
                var hasModifiedChanges = newData.Modified != oldData.Modified;

                if (hasBlobChanges) OnChanged(nameof(Blob));
                if (hasModifiedChanges) OnChanged(nameof(Modified));

                return hasBlobChanges || hasModifiedChanges;
            }
        }

        public override string GetBlob(string defaultValue = null, string tag = null)
        {
            if (Query != null)
            {
                var path = Query.GetAbsolutePath();

                var localData = Query.App.Database.OfflineDatabase.GetLocalData(path);
                var syncData = Query.App.Database.OfflineDatabase.GetSyncData(path);

                return localData.Modified > syncData.Modified ? localData.Blob : syncData.Blob;
            }
            else
            {
                return blobHolder;
            }
        }

        public void StartRealtime(FirebaseQuery parent)
        {
            Query = new ChildQuery(parent.App, parent, () => Key);
            SetBlob(blobHolder, InitTag);
        }

        public void StopRealtime()
        {
            Query = null;
        }

        public bool ConsumeStream(StreamObject streamObject)
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
        }

        public void Delete()
        {
            DeleteValue(null);
        }

        public bool ModifyValue<T>(T value, string tag = null)
        {
            lock (this)
            {
                var deserialized = Helpers.DeserializeString(GetBlob(default, tag));
                if (deserialized == null) deserialized = new string[1];
                var encodedModified = DataTypeConverter.GetConverter<SmallDateTime>().Encode(CurrentDateTimeFactory());
                var adsDatas = Helpers.BlobSetValue(deserialized.Skip(1).ToArray(), ModifiedKey, encodedModified);
                var newEncodedData = new string[adsDatas.Length + 1];
                newEncodedData[0] = DataTypeConverter.GetConverter<T>().Encode(value);
                Array.Copy(adsDatas, 0, newEncodedData, 1, adsDatas.Length);
                return SetBlob(Helpers.SerializeString(newEncodedData), tag);
            }
        }

        public bool DeleteValue(string tag = null)
        {
            lock (this)
            {
                var deserialized = Helpers.DeserializeString(GetBlob(default, tag));
                if (deserialized == null) deserialized = new string[1];
                deserialized[0] = null;
                return SetBlob(Helpers.SerializeString(deserialized), tag);
            }
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

        public override bool SetBlob(string blob, string tag = null)
        {
            var oldValue = GetRawValue();
            var hasChanges = base.SetBlob(blob, tag);
            if (oldValue != GetRawValue()) OnChanged(nameof(Value));
            return hasChanges;
        }

        #endregion
    }
}
