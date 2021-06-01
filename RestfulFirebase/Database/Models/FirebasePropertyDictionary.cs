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

        protected override (string key, FirebaseProperty value) ValueFactory(string key, FirebaseProperty value)
        {
            value.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(value.Property))
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
            };
            if (ModelWire != null)
            {
                if (value.ModelWire != null) value.ModelWire.Unsubscribe();
                ModelWire.RealtimeInstance.Child(key).PutModel(value);
            }
            return (key, value);
        }

        protected FirebaseProperty PropertyFactory(string key)
        {
            return new FirebaseProperty();
        }

        public void Dispose()
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
                    var prop = this.FirstOrDefault(i => i.Key == key);
                    if (prop.Value == null)
                    {
                        var item = PropertyFactory(key);
                        if (item == null) return;
                        var propPair = ValueFactory(key, item);
                        prop = new KeyValuePair<string, FirebaseProperty>(propPair.key, propPair.value);
                        ModelWire.RealtimeInstance.Child(key).SubModel(prop.Value);
                    }
                }
            });

            var props = this.ToList();
            var paths = ModelWire.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

            foreach (var prop in props)
            {
                if (invokeSetFirst) ModelWire.RealtimeInstance.Child(prop.Key).PutModel(prop.Value);
                else ModelWire.RealtimeInstance.Child(prop.Key).SubModel(prop.Value);
                paths.RemoveAll(i => i == prop.Key);
            }

            foreach (var path in paths)
            {
                var item = PropertyFactory(path);
                if (item == null) return;
                ModelWire.RealtimeInstance.Child(path).SubModel(item);
                Add(path, item);
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
            value.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(value.Property))
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
            };
            if (ModelWire != null)
            {
                if (value.ModelWire != null) value.ModelWire.Unsubscribe();
                ModelWire.RealtimeInstance.Child(key).PutModel(value);
            }
            return (key, value);
        }

        protected T PropertyFactory(string key)
        {
            return itemInitializer?.Invoke(key);
        }

        public void Dispose()
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
                    var prop = this.FirstOrDefault(i => i.Key == key);
                    if (prop.Value == null)
                    {
                        var item = PropertyFactory(key);
                        if (item == null) return;
                        var propPair = ValueFactory(key, item);
                        prop = new KeyValuePair<string, T>(propPair.key, propPair.value);
                        ModelWire.RealtimeInstance.Child(key).SubModel(prop.Value);
                    }
                }
            });

            var props = this.ToList();
            var paths = ModelWire.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

            foreach (var prop in props)
            {
                if (invokeSetFirst) ModelWire.RealtimeInstance.Child(prop.Key).PutModel(prop.Value);
                else ModelWire.RealtimeInstance.Child(prop.Key).SubModel(prop.Value);
                paths.RemoveAll(i => i == prop.Key);
            }

            foreach (var path in paths)
            {
                var item = PropertyFactory((path));
                if (item == null) continue;
                ModelWire.RealtimeInstance.Child(path).SubModel(item);
                Add(path, item);
            }
        }

        #endregion
    }
}
