using RestfulFirebase.Common;
using RestfulFirebase.Common.Conversions;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseProperty : DistinctProperty, IRealtimeModel
    {
        #region Properties

        public bool HasRealtimeWire => RealtimeWirePath != null;

        public string RealtimeWirePath
        {
            get => Holder.GetAttribute<string>(nameof(RealtimeWirePath), nameof(FirebaseProperty)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeWirePath), nameof(FirebaseProperty), value);
        }

        public IDisposable RealtimeSubscription
        {
            get => Holder.GetAttribute<IDisposable>(nameof(RealtimeSubscription), nameof(FirebaseProperty)).Value;
            internal set => Holder.SetAttribute(nameof(RealtimeSubscription), nameof(FirebaseProperty), value);
        }

        public DateTime Modified
        {
            get => Helpers.DecodeDateTime(GetAdditional("_m"), default);
            set => SetAdditional("_m", Helpers.EncodeDateTime(value));
        }

        #endregion

        #region Initializers

        public static new FirebaseProperty CreateFromKey(string key)
        {
            return new FirebaseProperty(DistinctProperty.CreateFromKey(key));
        }

        public static new FirebaseProperty CreateFromKeyAndValue<T>(string key, T value)
        {
            return new FirebaseProperty(DistinctProperty.CreateFromKeyAndValue(key, value));
        }

        public static new FirebaseProperty CreateFromKeyAndData(string key, string data)
        {
            return new FirebaseProperty(DistinctProperty.CreateFromKeyAndData(key, data));
        }

        public FirebaseProperty(IAttributed attributed)
            : base(attributed)
        {

        }

        public void Dispose()
        {
            RealtimeSubscription?.Dispose();
        }

        #endregion

        #region Methods

        public void SetRealtime(IFirebaseQuery query, RealtimeConfig config)
        {
            RealtimeWirePath = query.GetAbsolutePath();
            var oldDataFactory = DataFactory;
            DataFactory = new DataFactory(args =>
            {
                query.App.Database.OfflineDatabase.SetSyncData(RealtimeWirePath, args.Value);
            }, args =>
            {
                var sync = query.App.Database.OfflineDatabase.GetSyncData(RealtimeWirePath);
                var local = query.App.Database.OfflineDatabase.GetLocalData(RealtimeWirePath);
            });
            switch (config.InitialStrategy)
            {
                case InitialStrategy.Pull:
                    break;
                case InitialStrategy.Push:
                    break;
            }
            RealtimeSubscription = Observable
                .Create<StreamEvent>(observer => new NodeStreamer(observer, query, (s, e) => OnError(e)).Run())
                .Subscribe(streamEvent =>
                {
                    try
                    {
                        if (streamEvent.Path == null) throw new Exception("StreamEvent Key null");
                        else if (streamEvent.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                        else if (streamEvent.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                        else if (streamEvent.Path.Length == 1)
                        {
                            if (streamEvent.Data == null) Null();
                            else Update(streamEvent.Data);
                        }
                    }
                    catch (Exception ex)
                    {
                        OnError(ex);
                    }
                });
        }

        #endregion
    }
}
