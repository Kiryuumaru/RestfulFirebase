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
    public class FirebasePropertyGroup : DistinctGroup<FirebaseProperty>, IRealtimeModel
    {
        #region Properties

        public bool HasRealtimeWire => RealtimeSubscription != null;

        public string RealtimeWirePath
        {
            get => Holder.GetAttribute<string>(nameof(RealtimeWirePath), nameof(FirebasePropertyGroup)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWirePath), nameof(FirebasePropertyGroup), value);
        }

        public IDisposable RealtimeSubscription
        {
            get => Holder.GetAttribute<IDisposable>(nameof(RealtimeSubscription), nameof(FirebasePropertyGroup)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeSubscription), nameof(FirebasePropertyGroup), value);
        }

        #endregion

        #region Initializers

        public static new FirebasePropertyGroup CreateFromKey(string key)
        {
            return new FirebasePropertyGroup(DistinctGroup<FirebaseProperty>.CreateFromKey(key));
        }

        public static new FirebasePropertyGroup CreateFromKeyAndEnumerable(string key, IEnumerable<FirebaseProperty> properties)
        {
            return new FirebasePropertyGroup(DistinctGroup<FirebaseProperty>.CreateFromKeyAndEnumerable(key, properties));
        }

        public FirebasePropertyGroup(IAttributed attributed)
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
