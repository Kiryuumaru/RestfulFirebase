using RestfulFirebase.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class RealtimeHolder<T>
        where T : IRealtimeModel
    {
        public T RealtimeModel { get; private set; }

        private Action onStart;

        internal RealtimeHolder(T realtime, Action starter)
        {
            RealtimeModel = realtime;
            onStart = starter;
        }

        public void Start()
        {
            onStart?.Invoke();
        }
    }
}
