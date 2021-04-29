using Newtonsoft.Json;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebasePropertyGroup : ObservableGroup<FirebaseProperty>, IRealtimeModel
    {
        #region Properties

        public RealtimeWire Wire
        {
            get => Holder.GetAttribute<RealtimeWire>();
            set => Holder.SetAttribute(value);
        }

        public string Key
        {
            get => Holder.GetAttribute<string>();
            set => Holder.SetAttribute(value);
        }

        #endregion

        #region Initializers

        public FirebasePropertyGroup(IAttributed attributed)
            : base(attributed)
        {

        }

        public FirebasePropertyGroup(string key)
            : base()
        {
            Key = key;
        }

        #endregion

        #region Methods

        protected FirebaseProperty PropertyFactory(string key)
        {
            var newObj = new FirebaseProperty(key);
            if (Wire != null)
            {
                var subWire = Wire.Child(newObj.Key);
                newObj.MakeRealtime(subWire);
                subWire.InvokeStart();
            }
            return newObj;
        }

        public void MakeRealtime(RealtimeWire wire)
        {
            wire.OnStart += delegate
            {
                Wire = wire;
                foreach (var prop in this)
                {
                    var subWire = Wire.Child(prop.Key);
                    prop.MakeRealtime(subWire);
                    subWire.InvokeStart();
                }
            };
            wire.OnStop += delegate
            {
                Wire = null;
                foreach (var prop in this)
                {
                    prop.Wire.InvokeStop();
                }
            };
            wire.OnStream += streamObject =>
            {
                bool hasChanges = false;
                try
                {
                    if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                    else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                    else if (streamObject.Path[0] != Key) throw new Exception("StreamEvent Key mismatch");
                    else if (streamObject.Path.Length == 1)
                    {
                        var data = streamObject.Data == null ? new Dictionary<string, object>() : JsonConvert.DeserializeObject<Dictionary<string, object>>(streamObject.Data);
                        var blobs = data.Select(i => (i.Key, i.Value?.ToString()));
                        foreach (var prop in new List<FirebaseProperty>(this.Where(i => !blobs.Any(j => j.Key == i.Key))))
                        {
                            if (prop.Wire.InvokeStream(new StreamObject(null, prop.Key)))
                            {
                                Remove(prop);
                                hasChanges = true;
                            }
                        }
                        foreach (var blob in blobs)
                        {
                            try
                            {
                                var prop = this.FirstOrDefault(i => i.Key.Equals(blob.Key));

                                if (prop == null)
                                {
                                    prop = PropertyFactory(blob.Key);
                                    prop.Wire.InvokeStart();
                                    Add(prop);
                                    hasChanges = true;
                                }
                                else
                                {
                                    if (prop.Wire.InvokeStream(new StreamObject(blob.Item2, blob.Key)))
                                    {
                                        hasChanges = true;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                OnError(ex);
                            }
                        }
                    }
                    else if (streamObject.Path.Length == 2)
                    {
                        try
                        {
                            var prop = this.FirstOrDefault(i => i.Key.Equals(streamObject.Path[1]));

                            if (prop == null)
                            {
                                if (streamObject.Data == null) return false;
                                prop = PropertyFactory(streamObject.Path[1]);
                                prop.Wire.InvokeStart();
                                Add(prop);
                                hasChanges = true;
                            }
                            else
                            {
                                if (prop.Wire.InvokeStream(new StreamObject(streamObject.Data, streamObject.Path[1])))
                                {
                                    hasChanges = true;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            OnError(ex);
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                }
                return hasChanges;
            };
        }

        public void Delete()
        {
            foreach (var prop in new List<FirebaseProperty>(this))
            {
                prop.Delete();
                this.Remove(prop);
            }
        }


        #endregion
    }
}
