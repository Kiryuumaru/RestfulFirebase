using ObservableHelpers.Serializers;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public enum DataChangesType
    {
        Create, Update, Delete, None
    }

    public class DataChanges
    {
        public string Blob { get; }
        public DataChangesType ChangesType { get; }
        public long SyncPriority { get; }

        public static DataChanges Parse(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            var deserialized = Utils.DeserializeString(data);
            if (deserialized == null) return null;
            if (deserialized.Length != 3) return null;
            var changesType = DataChangesType.None;
            switch (deserialized[1])
            {
                case "1":
                    changesType = DataChangesType.Create;
                    break;
                case "2":
                    changesType = DataChangesType.Update;
                    break;
                case "3":
                    changesType = DataChangesType.Delete;
                    break;
            }
            var syncPriority = Serializer.Deserialize<long>(deserialized[2]);
            return new DataChanges(deserialized[0], changesType, syncPriority);
        }

        public DataChanges(string blob, DataChangesType changesType, long syncPriority)
        {
            Blob = blob;
            ChangesType = changesType;
            SyncPriority = syncPriority;
        }

        public string ToData()
        {
            var changesType = "0";
            switch (ChangesType)
            {
                case DataChangesType.Create:
                    changesType = "1";
                    break;
                case DataChangesType.Update:
                    changesType = "2";
                    break;
                case DataChangesType.Delete:
                    changesType = "3";
                    break;
            }
            return Utils.SerializeString(Blob, changesType, Serializer.Serialize(SyncPriority));
        }
    }
}
