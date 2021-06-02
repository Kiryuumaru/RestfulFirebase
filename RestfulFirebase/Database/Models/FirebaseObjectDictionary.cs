using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObjectDictionary : ObservableDictionary<string, FirebaseObject>, IRealtimeModelProxy
    {
        #region Properties

        internal RealtimeModelWire ModelWire { get; private set; }

        #endregion

        #region Methods

        public virtual void Dispose()
        {
            ModelWire?.Unsubscribe();
            ModelWire = null;
        }

        protected virtual void WireValue(string key, FirebaseObject value, bool invokeSetFirst)
        {
            value.PropertyChanged += (s, e) =>
            {
                lock (this)
                {
                    if (value.IsPersistablePropertiesNull())
                    {
                        if (this.ContainsKey(key))
                        {
                            Remove(key);
                        }
                    }
                    else
                    {
                        if (!this.ContainsKey(key))
                        {
                            Add(key, value);
                        }
                    }
                }
            };
            if (invokeSetFirst) ModelWire.RealtimeInstance.Child(key).PutModel(value);
            else ModelWire.RealtimeInstance.Child(key).SubModel(value);
        }

        protected override (string key, FirebaseObject value) ValueFactory(string key, FirebaseObject value)
        {
            if (ModelWire != null)
            {
                if (value.ModelWire == null)
                {
                    WireValue(key, value, true);
                }
                else if (value.ModelWire.RealtimeInstance.Parent != ModelWire.RealtimeInstance) throw new Exception("Item has different existing wire");
            }
            return (key, value);
        }

        protected override bool ValueRemove(string key, out FirebaseObject value)
        {
            var result = base.ValueRemove(key, out value);
            if (result && !value.IsPersistablePropertiesNull()) value.SetPersistablePropertiesNull();
            return result;
        }

        protected FirebaseObject ObjectFactory(string key)
        {
            return new FirebaseObject();
        }

        void IRealtimeModelProxy.StartRealtime(RealtimeModelWire modelWire, bool invokeSetFirst)
        {
            if (ModelWire != null)
            {
                ModelWire?.Unsubscribe();
                ModelWire = null;
            }

            ModelWire = modelWire;

            ModelWire.Subscribe();

            ModelWire.SetOnChanges(args =>
            {
                if (!string.IsNullOrEmpty(args.Path))
                {
                    var separated = Utils.UrlSeparate(args.Path);
                    var key = separated[0];
                    var dataCount = ModelWire.RealtimeInstance.Child(key).GetTotalDataCount();
                    KeyValuePair<string, FirebaseObject> obj;
                    lock (this)
                    {
                        obj = this.FirstOrDefault(i => i.Key == key);
                    }
                    if (obj.Value == null && dataCount != 0)
                    {
                        var item = ObjectFactory(key);
                        if (item == null) return;
                        WireValue(key, item, false);
                        lock (this)
                        {
                            Add(key, item);
                        }
                    }
                }
            });

            List<KeyValuePair<string, FirebaseObject>> objs = new List<KeyValuePair<string, FirebaseObject>>();
            lock (this)
            {
                objs = this.ToList();
            }
            var paths = ModelWire.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

            foreach (var obj in objs)
            {
                if (invokeSetFirst) ModelWire.RealtimeInstance.Child(obj.Key).PutModel(obj.Value);
                else ModelWire.RealtimeInstance.Child(obj.Key).SubModel(obj.Value);
                paths.RemoveAll(i => i == obj.Key);
            }

            foreach (var path in paths)
            {
                lock (this)
                {
                    if (this.Any(i => i.Key == path)) continue;
                }
                var item = ObjectFactory(path);
                if (item == null) return;
                ModelWire.RealtimeInstance.Child(path).SubModel(item);
                lock (this)
                {
                    Add(path, item);
                }
            }
        }

        #endregion
    }

    public class FirebaseObjectDictionary<T> : ObservableDictionary<string, T>, IRealtimeModelProxy
        where T : FirebaseObject
    {
        #region Properties

        internal RealtimeModelWire ModelWire { get; private set; }

        private Func<string, T> itemInitializer;

        #endregion

        #region Initializer

        public FirebaseObjectDictionary(Func<string, T> itemInitializer)
        {
            this.itemInitializer = itemInitializer;
        }

        #endregion

        #region Methods

        protected virtual void WireValue(string key, T value, bool invokeSetFirst)
        {
            value.PropertyChanged += (s, e) =>
            {
                lock (this)
                {
                    if (value.IsPersistablePropertiesNull())
                    {
                        if (this.ContainsKey(key))
                        {
                            Remove(key);
                        }
                    }
                    else
                    {
                        if (!this.ContainsKey(key))
                        {
                            Add(key, value);
                        }
                    }
                }
            };
            if (invokeSetFirst) ModelWire.RealtimeInstance.Child(key).PutModel(value);
            else ModelWire.RealtimeInstance.Child(key).SubModel(value);
        }

        protected override (string key, T value) ValueFactory(string key, T value)
        {
            if (ModelWire != null)
            {
                if (value.ModelWire == null)
                {
                    WireValue(key, value, true);
                }
                else if (value.ModelWire.RealtimeInstance.Parent != ModelWire.RealtimeInstance) throw new Exception("Item has different existing wire");
            }
            return (key, value);
        }

        protected override bool ValueRemove(string key, out T value)
        {
            var result = base.ValueRemove(key, out value);
            if (result && !value.IsPersistablePropertiesNull()) value.SetPersistablePropertiesNull();
            return result;
        }

        protected T ObjectFactory(string key)
        {
            return itemInitializer?.Invoke((key));
        }

        public virtual void Dispose()
        {
            ModelWire?.Unsubscribe();
            ModelWire = null;
        }

        void IRealtimeModelProxy.StartRealtime(RealtimeModelWire modelWire, bool invokeSetFirst)
        {
            if (ModelWire != null)
            {
                ModelWire?.Unsubscribe();
                ModelWire = null;
            }

            ModelWire = modelWire;

            ModelWire.Subscribe();

            ModelWire.SetOnChanges(args =>
            {
                if (!string.IsNullOrEmpty(args.Path))
                {
                    var separated = Utils.UrlSeparate(args.Path);
                    var key = separated[0];
                    var dataCount = ModelWire.RealtimeInstance.Child(key).GetTotalDataCount();
                    KeyValuePair<string, T> obj;
                    lock (this)
                    {
                        obj = this.FirstOrDefault(i => i.Key == key);
                    }
                    if (obj.Value == null && dataCount != 0)
                    {
                        var item = ObjectFactory(key);
                        if (item == null) return;
                        WireValue(key, item, false);
                        lock (this)
                        {
                            Add(key, item);
                        }
                    }
                }
            });

            List<KeyValuePair<string, T>> objs = new List<KeyValuePair<string, T>>();
            lock (this)
            {
                objs = this.ToList();
            }
            var paths = ModelWire.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

            foreach (var obj in objs)
            {
                if (invokeSetFirst) ModelWire.RealtimeInstance.Child(obj.Key).PutModel(obj.Value);
                else ModelWire.RealtimeInstance.Child(obj.Key).SubModel(obj.Value);
                paths.RemoveAll(i => i == obj.Key);
            }

            foreach (var path in paths)
            {
                lock (this)
                {
                    if (this.Any(i => i.Key == path)) continue;
                }
                var item = ObjectFactory(path);
                if (item == null) continue;
                ModelWire.RealtimeInstance.Child(path).SubModel(item);
                lock (this)
                {
                    Add(path, item);
                }
            }
        }

        #endregion
    }
}
