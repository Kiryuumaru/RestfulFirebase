using RestfulFirebase.Common;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineData
    {
        public string Blob { get; private set; }
        public SmallDateTime Modified { get; private set; }

        public static OfflineData Parse(FirebaseObject obj)
        {
            return new OfflineData(obj.Blob, obj.Modified);
        }

        public static OfflineData Parse(string data)
        {
            if (string.IsNullOrEmpty(data)) return null;
            var datas = Helpers.DeserializeString(data);
            if (datas == null) return null;
            if (datas.Length != 2) return null;
            var blob = datas[0];
            var modified = Helpers.DecodeSmallDateTime(datas[1], default);
            return new OfflineData(blob, modified);
        }

        public OfflineData(string blob, SmallDateTime modified)
        {
            Blob = blob;
            Modified = modified;
        }

        public string ToData()
        {
            return Helpers.SerializeString(Blob, Helpers.EncodeSmallDateTime(Modified));
        }
    }
}
