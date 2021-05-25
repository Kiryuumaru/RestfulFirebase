using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Realtime;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class RealtimeModelWireChangesEventArgs : EventArgs
    {
        public string Path { get; private set; }

        public RealtimeModelWireChangesEventArgs(string path)
        {
            Path = path;
        }
    }

    public class RealtimeModelWire
    {
        public RealtimeWire Wire { get; }
        public IRealtimeModel Model { get; }
        public string Path { get; }

        private Action<RealtimeModelWireChangesEventArgs> onChanges;

        internal RealtimeModelWire(
            RealtimeWire wire,
            IRealtimeModel model,
            string path)
        {
            Wire = wire;
            Model = model;
            Path = path;

            Path?.Trim();
            Path?.Trim('/');
        }

        public RealtimeModelWire Child(IRealtimeModel model, string subPath)
        {
            return new RealtimeModelWire(Wire, model, GetSubPath(subPath));
        }

        public bool SetBlob(string blob, string subPath = null)
        {
            return Wire.SetBlob(blob, GetSubPath(subPath));
        }

        public string GetBlob(string subPath = null)
        {
            return Wire.GetBlob(GetSubPath(subPath));
        }

        public IEnumerable<string> GetPaths(string subPath = null)
        {
            return Wire.GetPaths(GetSubPath(subPath));
        }

        public void SetOnChanges(Action<RealtimeModelWireChangesEventArgs> onChanges)
        {
            this.onChanges = onChanges;
        }

        internal void Subscribe()
        {
            Wire.OnInternalChanges += OnInternalChanges;
            Wire.OnInternalSync += OnInternalSync;
            Wire.OnInternalError += OnInternalError;
        }

        internal void Unsubscribe()
        {
            Wire.OnInternalChanges -= OnInternalChanges;
            Wire.OnInternalSync -= OnInternalSync;
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

        private void OnInternalSync(object sender, SyncEventArgs e)
        {

        }

        private void OnInternalError(object sender, Exception e)
        {
            Model.OnError(e);
        }
    }
}
