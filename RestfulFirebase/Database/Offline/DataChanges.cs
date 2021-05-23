using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    internal enum DataChangesType
    {
        Create, Update, Delete, None
    }

    internal class DataChanges
    {
        public string Blob { get; }
        public DataChangesType ChangesType { get; }

        public static DataChanges Parse(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            var deserialized = Utils.DeserializeString(data);
            if (deserialized == null) return null;
            if (deserialized.Length != 2) return null;
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
            return new DataChanges(deserialized[0], changesType);
        }

        public DataChanges(string blob, DataChangesType changesType)
        {
            Blob = blob;
            ChangesType = changesType;
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
            return Utils.SerializeString(Blob, changesType);
        }
    }
}
