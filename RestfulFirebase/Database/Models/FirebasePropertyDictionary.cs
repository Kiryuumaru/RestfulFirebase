using ObservableHelpers;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Models
{
    public class FirebasePropertyDictionary : ObservableDictionary<string, FirebaseProperty>, IRealtimeModelProxy
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

        protected override (string key, FirebaseProperty value) ValueFactory(string key, FirebaseProperty value)
        {
            if (ModelWire != null)
            {
                if (value.ModelWire == null)
                {
                    value.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(value.Property))
                        {
                            lock (this)
                            {
                                if (value.IsNull())
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
                        }
                    };
                    ModelWire.RealtimeInstance.Child(key).PutModel(value);
                }
                else if (value.ModelWire.RealtimeInstance.Parent != ModelWire.RealtimeInstance) throw new Exception("Item has different existing wire");
            }
            return (key, value);
        }

        protected override bool ValueRemove(string key, out FirebaseProperty value)
        {
            var result = base.ValueRemove(key, out value);
            if (result) value.SetNull();
            return result;
        }

        protected FirebaseProperty PropertyFactory(string key)
        {
            return new FirebaseProperty();
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
                    KeyValuePair<string, FirebaseProperty> prop;
                    lock (this)
                    {
                        prop = this.FirstOrDefault(i => i.Key == key);
                    }
                    if (prop.Value == null)
                    {
                        var item = PropertyFactory(key);
                        if (item == null) return;
                        item.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(item.Property))
                            {
                                lock (this)
                                {
                                    if (item.IsNull())
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
                                            Add(key, item);
                                        }
                                    }
                                }
                            }
                        };
                        ModelWire.RealtimeInstance.Child(key).SubModel(item);
                        lock (this)
                        {
                            Add(key, item);
                        }
                    }
                }
            });

            List<KeyValuePair<string, FirebaseProperty>> props = new List<KeyValuePair<string, FirebaseProperty>>();
            lock (this)
            {
                props = this.ToList();
            }
            var paths = ModelWire.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

            foreach (var prop in props)
            {
                if (invokeSetFirst) ModelWire.RealtimeInstance.Child(prop.Key).PutModel(prop.Value);
                else ModelWire.RealtimeInstance.Child(prop.Key).SubModel(prop.Value);
                paths.RemoveAll(i => i == prop.Key);
            }

            foreach (var path in paths)
            {
                lock (this)
                {
                    if (this.Any(i => i.Key == path)) continue;
                }
                var item = PropertyFactory(path);
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

    public class FirebasePropertyDictionary<T> : ObservableDictionary<string, T>, IRealtimeModelProxy
        where T : FirebaseProperty
    {
        #region Properties

        internal RealtimeModelWire ModelWire { get; private set; }

        private Func<string, T> itemInitializer;

        #endregion

        #region Initializer

        public FirebasePropertyDictionary(Func<string, T> itemInitializer)
        {
            this.itemInitializer = itemInitializer;
        }

        #endregion

        #region Methods

        protected override (string key, T value) ValueFactory(string key, T value)
        {
            if (ModelWire != null)
            {
                if (value.ModelWire == null)
                {
                    value.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(value.Property))
                        {
                            lock (this)
                            {
                                if (value.IsNull())
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
                        }
                    };
                    ModelWire.RealtimeInstance.Child(key).PutModel(value);
                }
                else if (value.ModelWire.RealtimeInstance.Parent != ModelWire.RealtimeInstance) throw new Exception("Item has different existing wire");
            }
            return (key, value);
        }

        protected override bool ValueRemove(string key, out T value)
        {
            var result = base.ValueRemove(key, out value);
            if (result) value.SetNull();
            return result;
        }

        protected T PropertyFactory(string key)
        {
            return itemInitializer?.Invoke(key);
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
                    KeyValuePair<string, T> prop;
                    lock (this)
                    {
                        prop = this.FirstOrDefault(i => i.Key == key);
                    }
                    if (prop.Value == null)
                    {
                        var item = PropertyFactory(key);
                        if (item == null) return;
                        item.PropertyChanged += (s, e) =>
                        {
                            if (e.PropertyName == nameof(item.Property))
                            {
                                lock (this)
                                {
                                    if (item.IsNull())
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
                                            Add(key, item);
                                        }
                                    }
                                }
                            }
                        };
                        ModelWire.RealtimeInstance.Child(key).SubModel(item);
                        Add(key, item);
                    }
                }
            });

            List<KeyValuePair<string, T>> props = new List<KeyValuePair<string, T>>();
            lock (this)
            {
                props = this.ToList();
            }
            var paths = ModelWire.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

            foreach (var prop in props)
            {
                if (invokeSetFirst) ModelWire.RealtimeInstance.Child(prop.Key).PutModel(prop.Value);
                else ModelWire.RealtimeInstance.Child(prop.Key).SubModel(prop.Value);
                paths.RemoveAll(i => i == prop.Key);
            }

            foreach (var path in paths)
            {
                lock (this)
                {
                    if (this.Any(i => i.Key == path)) continue;
                }
                var item = PropertyFactory((path));
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
