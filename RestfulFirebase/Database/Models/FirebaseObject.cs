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

        private string blobHolder = null;
        private RealtimeWire wire;
        private OfflineData lastPush;

        protected const string InitTag = "init";
        protected const string SyncTag = "sync";
        protected const string RevertTag = "revert";

        public const string ModifiedKey = "m";

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
            if (wire != null)
            {
                var path = wire.Query.GetAbsolutePath();

                void put(OfflineData data, string revertBlob)
                {
                    if (lastPush != null) if(lastPush.Modified >= data.Modified) return;
                    lastPush = data;
                    wire.Put(JsonConvert.SerializeObject(lastPush.Value == null ? null : lastPush.Blob), ex =>
                    {
                        if (wire == null) return;
                        if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            SetBlob(revertBlob, RevertTag);
                        }
                        OnError(ex);
                    });
                }

                var newData = new OfflineData(blob);
                var localData = wire.Query.App.Database.OfflineDatabase.GetLocalData(path);
                var syncData = wire.Query.App.Database.OfflineDatabase.GetSyncData(path);
                var currData = localData.Modified > syncData.Modified ? localData : syncData;

                switch (tag)
                {
                    case InitTag:
                        if (newData.Modified <= SmallDateTime.MinValue) return false;
                        return wire.Query.App.Database.OfflineDatabase.SetLocalData(path, newData);
                    case RevertTag:
                        wire.Query.App.Database.OfflineDatabase.SetLocalData(path, newData);
                        break;
                    case SyncTag:
                        if (newData.Blob == null)
                        {
                            if (syncData.Value != null && localData.Value != null)
                            {
                                if (syncData.Modified < localData.Modified)
                                {
                                    put(localData, currData.Blob);
                                }
                                else
                                {
                                    wire.Query.App.Database.OfflineDatabase.DeleteSyncData(path);
                                    wire.Query.App.Database.OfflineDatabase.DeleteLocalData(path);
                                }
                            }
                            else if (syncData.Value != null && localData.Value == null)
                            {
                                wire.Query.App.Database.OfflineDatabase.DeleteSyncData(path);
                                wire.Query.App.Database.OfflineDatabase.DeleteLocalData(path);
                                return false;
                            }
                            else if (syncData.Value == null && localData.Value != null)
                            {
                                put(localData, currData.Blob);
                            }
                            else
                            {
                                return false;
                            }
                        }
                        else
                        {
                            if (currData.Modified <= newData.Modified)
                            {
                                wire.Query.App.Database.OfflineDatabase.SetSyncData(path, newData);
                            }
                            else
                            {
                                put(currData, currData.Blob);
                            }
                        }
                        break;
                    default:
                        if (newData.Value == null && currData.Blob == null) return false;
                        if (newData.Modified >= currData.Modified)
                        {
                            put(newData, currData.Blob);
                            wire.Query.App.Database.OfflineDatabase.SetLocalData(path, newData);
                        }
                        break;
                }

                var newLocalData = wire.Query.App.Database.OfflineDatabase.GetLocalData(path);
                var newSyncData = wire.Query.App.Database.OfflineDatabase.GetSyncData(path);
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
            if (wire != null)
            {
                var path = wire.Query.GetAbsolutePath();

                var localData = wire.Query.App.Database.OfflineDatabase.GetLocalData(path);
                var syncData = wire.Query.App.Database.OfflineDatabase.GetSyncData(path);

                return localData.Modified > syncData.Modified ? localData.Blob : syncData.Blob;
            }
            else
            {
                return blobHolder;
            }
        }

        public void MakeRealtime(RealtimeWire wire)
        {
            wire.OnStart += delegate
            {
                this.wire = wire;
                SetBlob(blobHolder, InitTag);
            };
            wire.OnStop += delegate
            {
                this.wire = null;
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
            DeleteValue(null);
        }

        public bool ModifyValue<T>(T value, string tag = null)
        {
            lock (this)
            {
                var deserialized = Helpers.DeserializeString(GetBlob(default, tag));
                if (deserialized == null) deserialized = new string[1];
                var encodedValue = DataTypeConverter.GetConverter<T>().Encode(value);
                if (deserialized[0] == encodedValue) return false;
                var encodedModified = DataTypeConverter.GetConverter<SmallDateTime>().Encode(CurrentDateTimeFactory());
                var adsDatas = Helpers.BlobSetValue(deserialized.Skip(1).ToArray(), ModifiedKey, encodedModified);
                var newEncodedData = new string[adsDatas.Length + 1];
                newEncodedData[0] = encodedValue;
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
                if (deserialized[0] == null) return false;
                var encodedModified = DataTypeConverter.GetConverter<SmallDateTime>().Encode(CurrentDateTimeFactory());
                var adsDatas = Helpers.BlobSetValue(deserialized.Skip(1).ToArray(), ModifiedKey, encodedModified);
                var newEncodedData = new string[adsDatas.Length + 1];
                newEncodedData[0] = null;
                Array.Copy(adsDatas, 0, newEncodedData, 1, adsDatas.Length);
                return SetBlob(Helpers.SerializeString(newEncodedData), tag);
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
