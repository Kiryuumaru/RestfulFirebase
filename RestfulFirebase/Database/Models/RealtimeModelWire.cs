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
        internal RealtimeIntance Wire { get; }

        internal IRealtimeModelProxy Model { get; }

        internal string Path { get; }

        internal bool Subscribed { get; private set; }

        private Action onSubscribe;
        private Action onUnsubscribe;
        private Action<RealtimeModelWireChangesEventArgs> onChanges;

        internal RealtimeModelWire(
            RealtimeIntance wire,
            IRealtimeModelProxy model,
            string path)
        {
            Wire = wire;
            Model = model;
            Path = path;

            Path?.Trim();
            Path?.Trim('/');
        }

        internal bool SetBlob(string blob)
        {
            return Wire.SetBlob(blob);
        }

        internal string GetBlob()
        {
            return Wire.GetBlob();
        }

        internal IEnumerable<string> GetPaths()
        {
            return Wire.GetPaths();
        }

        internal void SetOnSubscribed(Action onSubscribe)
        {
            this.onSubscribe = onSubscribe;
        }

        internal void SetOnUnsubscribe(Action onUnsubscribe)
        {
            this.onUnsubscribe = onUnsubscribe;
        }

        internal void SetOnChanges(Action<RealtimeModelWireChangesEventArgs> onChanges)
        {
            this.onChanges = onChanges;
        }

        internal void Subscribe()
        {
            if (Subscribed) return;
            Subscribed = true;
            onSubscribe?.Invoke();
            Wire.OnInternalChanges += OnInternalChanges;
            Wire.OnInternalError += OnInternalError; ;
        }

        internal void Unsubscribe()
        {
            if (!Subscribed) return;
            Subscribed = false;
            onUnsubscribe?.Invoke();
            Wire.OnInternalChanges -= OnInternalChanges;
            Wire.OnInternalError -= OnInternalError;
        }

        private string GetSubPath(string subPath)
        {
            string path = null;

            if (string.IsNullOrEmpty(Path) && string.IsNullOrEmpty(subPath))
            {
                path = null;
            }
            else if (string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(subPath))
            {
                path = Utils.UrlCombine(subPath);
            }
            else if (!string.IsNullOrEmpty(Path) && string.IsNullOrEmpty(subPath))
            {
                path = Utils.UrlCombine(Path);
            }
            else if (!string.IsNullOrEmpty(Path) && !string.IsNullOrEmpty(subPath))
            {
                path = Utils.UrlCombine(Path, subPath);
            }

            return path;
        }

        private void OnInternalChanges(object sender, DataChangesEventArgs e)
        {
            string path = null;
            if (string.IsNullOrEmpty(Path))
            {
                path = e.Path;
            }
            else
            {
                if (Utils.UrlIsBaseFrom(Path, e.Path))
                {
                    path = e.Path.Replace(Path, "");
                }
            }

            if (path != null) onChanges?.Invoke(new RealtimeModelWireChangesEventArgs(path));
        }

        private void OnInternalError(object sender, WireErrorEventArgs e)
        {
            Model.OnError(e.Exception);
        }
    }
}
