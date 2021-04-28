using Newtonsoft.Json;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObjects : ObservableObjects, IRealtimeModel
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

        public FirebaseObjects(IAttributed attributed)
            : base(attributed)
        {

        }

        public FirebaseObjects(string key)
            : base()
        {
            Key = key;
        }

        #endregion

        #region Methods

        protected override PropertyHolder PropertyFactory(string key, string group, string propertyName)
        {
            var newObj = new FirebaseObject(key);
            if (Wire != null)
            {
                var subWire = Wire.Child(newObj.Key);
                newObj.MakeRealtime(subWire);
                subWire.InvokeStart();
            }
            return new PropertyHolder()
            {
                Property = newObj,
                Key = newObj.Key,
                Group = group,
                PropertyName = propertyName
            };
        }

        public void SetPersistableProperty<T>(
            T value,
            string key,
            [CallerMemberName] string propertyName = null,
            Func<T, T, bool> validateValue = null,
            Func<(T value, ObservableObject property), bool> customValueSetter = null)
        {
            base.SetProperty(value, key, nameof(FirebaseObjects), propertyName, validateValue, customValueSetter);
        }

        public T GetPersistableProperty<T>(
            string key,
            T defaultValue = default,
            [CallerMemberName] string propertyName = null,
            Func<(T value, ObservableObject property), bool> customValueSetter = null)
        {
            return base.GetProperty(key, nameof(FirebaseObjects), defaultValue, propertyName, customValueSetter);
        }

        public IEnumerable<FirebaseObject> GetRawPersistableProperties()
        {
            return GetRawProperties(nameof(FirebaseObjects)).Select(i => (FirebaseObject)i);
        }

        public void SetBlobs(IEnumerable<(string key, string blob)> blobs, bool replace = false)
        {

        }

        public void MakeRealtime(RealtimeWire wire)
        {
            wire.OnStart += delegate
            {
                Wire = wire;
                foreach (var prop in GetRawPersistableProperties())
                {
                    var subWire = Wire.Child(prop.Key);
                    prop.MakeRealtime(subWire);
                    subWire.InvokeStart();
                }
            };
            wire.OnStop += delegate
            {
                Wire = null;
                foreach (var prop in GetRawPersistableProperties())
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
                        foreach (var propHolder in new List<PropertyHolder>(PropertyHolders.Where(i => !blobs.Any(j => j.Key == i.Key))))
                        {
                            if (((FirebaseObject)propHolder.Property).Wire.InvokeStream(new StreamObject(null, propHolder.Key)))
                            {
                                OnChanged(propHolder.Key, propHolder.Group, propHolder.PropertyName);
                                hasChanges = true;
                            }
                        }
                        foreach (var blob in blobs)
                        {
                            try
                            {
                                bool hasSubChanges = false;

                                var propHolder = PropertyHolders.FirstOrDefault(i => i.Key.Equals(blob.Key));

                                if (propHolder == null)
                                {
                                    var prop = PropertyFactory(blob.Key, null, null);
                                    ((FirebaseObject)prop.Property).Wire.InvokeStart();
                                    PropertyHolders.Add(prop);
                                    hasSubChanges = true;
                                }
                                else
                                {
                                    if (((FirebaseObject)propHolder.Property).Wire.InvokeStream(new StreamObject(blob.Item2, blob.Key)))
                                    {
                                        hasSubChanges = true;
                                    }
                                }

                                if (hasSubChanges)
                                {
                                    OnChanged(propHolder.Key, propHolder.Group, propHolder.PropertyName);
                                    hasChanges = true;
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
                            bool hasSubChanges = false;

                            var propHolder = PropertyHolders.FirstOrDefault(i => i.Key.Equals(streamObject.Path[1]));

                            if (propHolder == null)
                            {
                                if (streamObject.Data == null) return false;
                                var prop = PropertyFactory(streamObject.Path[1], null, null);
                                ((FirebaseObject)prop.Property).Wire.InvokeStart();
                                PropertyHolders.Add(prop);
                                hasSubChanges = true;
                            }
                            else
                            {
                                if (((FirebaseObject)propHolder.Property).Wire.InvokeStream(new StreamObject(streamObject.Data, streamObject.Path[1])))
                                {
                                    hasSubChanges = true;
                                }
                            }

                            if (hasSubChanges)
                            {
                                OnChanged(propHolder.Key, propHolder.Group, propHolder.PropertyName);
                                hasChanges = true;
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
            foreach (var propHolder in PropertyHolders)
            {
                DeleteProperty(propHolder.Key);
            }
        }

        public T ParseModel<T>()
            where T : FirebaseObjects
        {
            return (T)Activator.CreateInstance(typeof(T), this);
        }

        #endregion
    }
}
