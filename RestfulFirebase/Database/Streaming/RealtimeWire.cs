﻿using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Query;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Text;

namespace RestfulFirebase.Database.Streaming
{
    public class RealtimeWire : IDisposable
    {
        private string jsonToPut;
        private bool invokePut = false;
        private bool isInvoking = false;

        protected IDisposable Subscription;

        protected void InvokeStart() => OnStart?.Invoke();
        protected void InvokeStop() => OnStop?.Invoke();
        protected bool InvokeStream(StreamObject streamObject) => OnStream?.Invoke(streamObject) ?? false;

        public FirebaseQuery Query { get; private set; }

        public event Action OnStart;
        public event Action OnStop;
        public event Func<StreamObject, bool> OnStream;

        internal RealtimeWire(string key, FirebaseQuery parent)
        {
            Query = new ChildQuery(parent.App, parent, () => key);
        }

        public async void Put(string json, Action<FirebaseException> onError)
        {
            if (jsonToPut == json) return;
            jsonToPut = json;
            invokePut = true;
            if (isInvoking) return;
            isInvoking = true;
            while (invokePut)
            {
                invokePut = false;
                await Query.Put(jsonToPut, null, onError);
            }
            isInvoking = false;
        }

        public virtual void Start()
        {
            InvokeStart();
        }

        public void Dispose()
        {
            Subscription?.Dispose();
            OnStop?.Invoke();
        }
    }

    public class RealtimeWire<T> : RealtimeWire
        where T : IRealtimeModel
    {
        public T Model { get; private set; }

        internal RealtimeWire(T model, FirebaseQuery parent)
            : base (model.Key, parent)
        {
            Model = model;
        }

        public override void Start()
        {
            Model.MakeRealtime(this);
            InvokeStart();
            Subscription = Observable
                .Create<StreamObject>(observer => new NodeStreamer(observer, Query, (s, e) => Model.OnError(e)).Run())
                .Subscribe(streamObject => { InvokeStream(streamObject); });
        }
    }
}
