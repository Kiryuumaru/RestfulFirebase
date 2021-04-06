using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public enum InitialStrategy
    {
        Pull, Push
    }

    public struct RealtimeConfig
    {
        public InitialStrategy InitialStrategy { get; set; }
    }
}
