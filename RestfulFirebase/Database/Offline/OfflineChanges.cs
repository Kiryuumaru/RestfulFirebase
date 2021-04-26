using RestfulFirebase.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public enum OfflineChangesType
    {
        Create, Update, Delete, None
    }

    public class OfflineChanges
    {
        public string Blob { get; }
        public OfflineChangesType ChangesType { get; }

        public static OfflineChanges Parse(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            var deserialized = Helpers.DeserializeString(data);
            if (deserialized == null) return null;
            if (deserialized.Length != 2) return null;
            var changesType = OfflineChangesType.None;
            switch (deserialized[1])
            {
                case "1":
                    changesType = OfflineChangesType.Create;
                    break;
                case "2":
                    changesType = OfflineChangesType.Update;
                    break;
                case "3":
                    changesType = OfflineChangesType.Delete;
                    break;
            }
            return new OfflineChanges(deserialized[0], changesType);
        }

        public OfflineChanges(string blob, OfflineChangesType changesType)
        {
            Blob = blob;
            ChangesType = changesType;
        }

        public string ToData()
        {
            var changesType = "0";
            switch (ChangesType)
            {
                case OfflineChangesType.Create:
                    changesType = "1";
                    break;
                case OfflineChangesType.Update:
                    changesType = "2";
                    break;
                case OfflineChangesType.Delete:
                    changesType = "3";
                    break;
            }
            return Helpers.SerializeString(Blob, changesType);
        }
    }
}
