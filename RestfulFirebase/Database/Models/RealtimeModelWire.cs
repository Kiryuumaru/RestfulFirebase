using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    internal class RealtimeModelWireChangesEventArgs : EventArgs
    {
        internal string Path { get; private set; }

        internal RealtimeModelWireChangesEventArgs(string path)
        {
            Path = path;
        }
    }

    internal class RealtimeModelWire
    {
        internal RealtimeIntance RealtimeInstance { get; }

        internal IRealtimeModelProxy Model { get; }

        internal bool Subscribed { get; private set; }

        private Action<RealtimeModelWireChangesEventArgs> onChanges;

        internal RealtimeModelWire(
            RealtimeIntance realtimeInstance,
            IRealtimeModelProxy model)
        {
            RealtimeInstance = realtimeInstance;
            Model = model;
        }

        internal bool SetBlob(string blob)
        {
            return RealtimeInstance.SetBlob(blob);
        }

        internal string GetBlob()
        {
            return RealtimeInstance.GetBlob();
        }

        internal IEnumerable<string> GetSubPaths()
        {
            return RealtimeInstance.GetSubPaths();
        }

        internal void SetOnChanges(Action<RealtimeModelWireChangesEventArgs> onChanges)
        {
            this.onChanges = onChanges;
        }

        internal void Subscribe()
        {
            if (Subscribed) return;
            Subscribed = true;
            RealtimeInstance.OnInternalChanges += OnInternalChanges;
            RealtimeInstance.OnInternalError += OnInternalError;
        }

        internal void Unsubscribe()
        {
            if (!Subscribed) return;
            Subscribed = false;
            RealtimeInstance.OnInternalChanges -= OnInternalChanges;
            RealtimeInstance.OnInternalError -= OnInternalError;
        }

        private void OnInternalChanges(object sender, DataChangesEventArgs e)
        {
            onChanges?.Invoke(new RealtimeModelWireChangesEventArgs(e.Path));
        }

        private void OnInternalError(object sender, WireErrorEventArgs e)
        {
            Model.OnError(e.Exception);
        }
    }
}
