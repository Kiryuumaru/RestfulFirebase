using RestfulFirebase.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineData
    {
        public string LastData { get; internal set; }
        public string CurrentData { get; internal set; }

        public static OfflineData Parse(string data)
        {
            var lastData = Helpers.BlobGetValue(data, "lastData");
            var currentData = Helpers.BlobGetValue(data, "currentData");
            return new OfflineData(lastData, currentData);
        }

        public OfflineData(string lastData, string currentData)
        {
            LastData = lastData;
            CurrentData = currentData;
        }

        public override string ToString()
        {
            var data = Helpers.BlobSetValue("", "lastData", LastData);
            data = Helpers.BlobSetValue(data, "currentData", CurrentData);
            return data;
        }
    }
}
