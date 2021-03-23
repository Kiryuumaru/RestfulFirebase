using Newtonsoft.Json;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObjectGroup : DistinctGroup<FirebaseObject>, IRealtimeModel
    {
        #region Properties

        public bool HasRealtimeWire => RealtimeSubscription != null;

        public string RealtimeWirePath
        {
            get => Holder.GetAttribute<string>(nameof(RealtimeWirePath), nameof(FirebaseObjectGroup)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWirePath), nameof(FirebaseObjectGroup), value);
        }

        public IDisposable RealtimeSubscription
        {
            get => Holder.GetAttribute<IDisposable>(nameof(RealtimeSubscription), nameof(FirebaseObjectGroup)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeSubscription), nameof(FirebaseObjectGroup), value);
        }

        #endregion

        #region Initializers

        public static new FirebaseObjectGroup CreateFromKey(string key)
        {
            return new FirebaseObjectGroup(DistinctGroup<FirebaseObject>.CreateFromKey(key));
        }

        public FirebaseObjectGroup(IAttributed attributed)
            : base(attributed)
        {

        }

        public void Dispose()
        {

        }

        #endregion

        #region Methods

        internal void ConsumePersistableStream(StreamEvent streamEvent)
        {
            if (streamEvent.Path == null) throw new Exception("StreamEvent Key null");
            else if (streamEvent.Path.Length == 0) throw new Exception("StreamEvent Key empty");
            else if (streamEvent.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
            else if (streamEvent.Path.Length == 1)
            {

            }
            else if (streamEvent.Path.Length == 2)
            {

            }
        }

        #endregion
    }
}
