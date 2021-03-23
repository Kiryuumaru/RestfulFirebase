using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public interface IRealtimeModel : IDisposable
    {
        bool HasRealtimeWire { get; }
        string RealtimeWirePath { get; }
        IDisposable RealtimeSubscription { get; }
    }
}
