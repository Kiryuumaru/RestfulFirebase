﻿using RestfulFirebase.Auth;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RestfulFirebase.Database.Realtime;
using ObservableHelpers;
using RestfulFirebase.Extensions;
using RestfulFirebase.Serializers;

namespace RestfulFirebase.Database.Models.Primitive
{
    public class FirebaseProperty : ObservableProperty, IRealtimeModel
    {
        #region Properties

        protected const string InitTag = "init";
        protected const string SyncTag = "sync";

        public RealtimeWire Wire { get; private set; }
        public DataNode Node { get; private set; }

        #endregion

        #region Methods

        private void OnPutError(RetryExceptionEventArgs err)
        {
            if (err.Exception is FirebaseException ex)
            {
                if (ex.Reason == FirebaseExceptionReason.DatabaseUnauthorized)
                {
                    var hasChanges = false;
                    if (Node.Sync == null)
                    {
                        if (Node.Delete()) hasChanges = true;
                    }
                    else
                    {
                        if (Node.DeleteChanges()) hasChanges = true;
                    }
                    if (hasChanges) OnChanged(nameof(Property));
                }
            }
            OnError(err.Exception);
        }

        protected virtual bool SetBlob(string blob, string tag = null)
        {
            bool hasChanges = false;

            if (Wire != null)
            {
                switch (tag)
                {
                    case InitTag:
                        if (Wire.InvokeSetFirst)
                        {
                            var obj = GetObject(null, tag);
                            if (obj is string strObj)
                            {
                                if (Node.MakeChanges(strObj, OnPutError)) hasChanges = true;
                                return hasChanges;
                            }
                            else
                            {
                                if (Node.MakeChanges(null, OnPutError)) hasChanges = true;
                                return hasChanges;
                            }
                        }
                        else
                        {
                            hasChanges = true;
                        }
                        break;
                    case SyncTag:
                        if (Node.MakeSync(blob, OnPutError)) hasChanges = true;
                        break;
                    default:
                        if (Node.MakeChanges(blob, OnPutError)) hasChanges = true;
                        break;
                }

                if (hasChanges) OnChanged(nameof(Property));
            }
            else
            {
                if (SetObject(blob)) hasChanges = true;
            }

            return hasChanges;
        }

        protected virtual string GetBlob(string defaultValue = null, string tag = null)
        {
            if (Wire != null)
            {
                return Node.Blob;
            }
            else
            {
                var obj = GetObject(defaultValue, tag);
                if (obj is string strObj)
                {
                    return strObj;
                }
                else
                {
                    return defaultValue;
                }
            }
        }

        public override bool SetValue<T>(T value, string tag = null)
        {
            if (typeof(T).IsAssignableFrom(typeof(FirebaseProperty)))
            {
                OnError(new Exception("Cannot nest assign FirebaseProperty"));
                return false;
            }
            else if (typeof(T).IsAssignableFrom(typeof(FirebaseObject)))
            {
                return SetObject(value, tag);
            }
            else
            {
                try
                {
                    var json = Serializer.Serialize(value);
                    return SetBlob(json, tag);
                }
                catch (Exception ex)
                {
                    OnError(ex);
                    return false;
                }
            }
        }

        public override T GetValue<T>(T defaultValue = default, string tag = null)
        {
            if (typeof(T).IsAssignableFrom(typeof(FirebaseProperty)))
            {
                OnError(new Exception("Cannot nest assign FirebaseProperty"));
                return defaultValue;
            }
            else if (typeof(T).IsAssignableFrom(typeof(FirebaseObject)))
            {
                var obj = GetObject(defaultValue, tag);
                if (obj is FirebaseObject) return (T)obj;
                else return defaultValue;
            }
            else
            {
                try
                {
                    var str = GetBlob(null, tag);
                    if (str == null)
                    {
                        return defaultValue;
                    }
                    else
                    {
                        return Serializer.Deserialize<T>(str);
                    }
                }
                catch (Exception ex)
                {
                    OnError(ex);
                    return defaultValue;
                }
            }
        }

        public void MakeRealtime(RealtimeWire wire)
        {
            wire.OnStart += delegate
            {
                Node = new DataNode(wire);
                Wire = wire;
                SetBlob(null, InitTag);
            };
            wire.OnStop += delegate
            {
                Node = null;
                Wire = null;
            };
            wire.OnStream += streamObject =>
            {
                bool hasChanges = false;
                try
                {
                    if (streamObject.Path == null) throw new Exception("StreamEvent Key null");
                    else if (streamObject.Path.Length == 0) throw new Exception("StreamEvent Key empty");
                    else if (streamObject.Path.Length == 1)
                    {
                        if (streamObject.Object is SingleStreamData single)
                        {
                            hasChanges = SetBlob(single.Data, SyncTag);
                        }
                        //else if (streamObject.Object is MultiStreamData multi)
                        //{
                        //    var obj = GetObject(new FirebaseO, SyncTag);
                        //    hasChanges = SetBlob(obj.Data, SyncTag);
                        //}
                        else if (streamObject.Object is null)
                        {
                            hasChanges = SetBlob(null, SyncTag);
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

        public bool Delete()
        {
            return SetBlob(null);
        }

        #endregion
    }

    public class FirebaseProperty<T> : FirebaseProperty
    {
        #region Properties

        public T Value
        {
            get => base.GetValue<T>();
            set => base.SetValue(value);
        }

        #endregion

        #region Methods

        protected override bool SetBlob(string blob, string tag = null)
        {
            var hasChanges = false;
            if (tag == InitTag)
            {
                if (Wire.InvokeSetFirst)
                {
                    if (base.SetBlob(blob, tag)) hasChanges = true;
                    return hasChanges;
                }
                else
                {
                    if (base.SetBlob(blob, tag)) hasChanges = true;
                }
            }
            else
            {
                if (base.SetBlob(blob, tag)) hasChanges = true;
            }

            if (hasChanges) OnChanged(nameof(Value));

            return hasChanges;
        }

        #endregion
    }
}
