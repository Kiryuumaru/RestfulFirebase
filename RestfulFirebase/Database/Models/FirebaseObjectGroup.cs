using Newtonsoft.Json;
using RestfulFirebase.Common;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObjectGroup : DistinctGroup<FirebaseObject>, IRealtimeModel
    {
        #region Properties

        public RealtimeWire RealtimeWire
        {
            get => Holder.GetAttribute<RealtimeWire>(nameof(RealtimeWire), nameof(FirebaseObjectGroup)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWire), nameof(FirebaseObjectGroup), value);
        }

        #endregion

        #region Initializers

        public static new FirebaseObjectGroup CreateFromKey(string key)
        {
            return new FirebaseObjectGroup(DistinctGroup<FirebaseObject>.CreateFromKey(key));
        }

        public static new FirebaseObjectGroup CreateFromKeyAndEnumerable(string key, IEnumerable<FirebaseObject> properties)
        {
            return new FirebaseObjectGroup(DistinctGroup<FirebaseObject>.CreateFromKeyAndEnumerable(key, properties));
        }

        public FirebaseObjectGroup(IAttributed attributed)
            : base(attributed)
        {

        }

        #endregion

        #region Methods

        public void Delete()
        {

        }

        public void BuildRealtimeWire(FirebaseQuery query, bool invokeSetFirst)
        {
            RealtimeWire = new RealtimeWire(query,
                () =>
                {

                },
                () =>
                {

                },
                streamObject =>
                {
                    bool hasChanges = false;
                    try
                    {
                        if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                        else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                        else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                        else if (streamObject.Path.Length == 1)
                        {
                            var data = streamObject.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(streamObject.Data);
                            var objs = data.Select(i => (i.Key, i.Value.ToString()));


                        }
                        else if (streamObject.Path.Length == 2)
                        {

                        }
                        else if (streamObject.Path.Length == 3)
                        {

                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
                    return hasChanges;
                });
        }

        public void DisposeRealtimeWire()
        {

        }

        #endregion
    }
}
