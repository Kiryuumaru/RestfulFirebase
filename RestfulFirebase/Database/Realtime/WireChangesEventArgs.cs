using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Realtime
{
    public class WireChangesEventArgs : EventArgs
    {
        public string[] Path { get; }

        public WireChangesEventArgs(string[] path)
        {
            Path = path;
        }
    }
}
