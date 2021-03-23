using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineAction
    {
        public string OldData { get; }
        public string NewData { get; }
        public long Priority { get; }
        public OfflineAction(string oldData, string newData, long priority)
        {
            OldData = oldData;
            NewData = newData;
            Priority = priority;
        }
    }
}
