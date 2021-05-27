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

        protected override (string key, FirebaseObject value) ValueFactory(string key, FirebaseObject value)
        {
            value.PropertyChanged += (s, e) =>
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
            };
            if (ModelWire != null)
            {
                if (value.ModelWire != null) value.ModelWire.Unsubscribe();
                ModelWire.RealtimeInstance.Child(key).PutModel(value);
            }
            return (key, value);
        }

        protected FirebaseObject ObjectFactory()
        {
            return new FirebaseObject();
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
                    var obj = this.FirstOrDefault(i => i.Key == key);
                    if (obj.Value == null)
                    {
                        var objPair = ValueFactory(key, ObjectFactory());
                        obj = new KeyValuePair<string, FirebaseObject>(objPair.key, objPair.value);
                        ModelWire.RealtimeInstance.Child(key).SubModel(obj.Value);
                    }
                }
            });

            var objs = this.ToList();
            var paths = ModelWire.GetSubPaths().Select(i => Utils.UrlSeparate(i)[0]).ToList();

            foreach (var obj in objs)
            {
                if (invokeSetFirst) ModelWire.RealtimeInstance.Child(obj.Key).PutModel(obj.Value);
                else ModelWire.RealtimeInstance.Child(obj.Key).SubModel(obj.Value);
                paths.RemoveAll(i => i == obj.Key);
            }

            foreach (var path in paths)
            {
                var prop = ObjectFactory();
                ModelWire.RealtimeInstance.Child(path).SubModel(prop);
                Add(path, prop);
            }
        }

        #endregion
    }
}
