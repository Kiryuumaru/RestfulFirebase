using RestfulFirebase.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineData
    {
        public string Data { get; private set; }

        public OfflineData(string data)
        {
            Data = data;
            IsEndNode = isEndNode;
        }
    }
}
