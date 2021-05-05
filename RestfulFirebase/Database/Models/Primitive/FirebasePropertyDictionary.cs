using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Models.Primitive;
using RestfulFirebase.Database.Streaming;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Database.Models.Primitive
{
    public class FirebasePropertyDictionary : ObservableDictionary<string, FirebaseProperty>, IRealtimeModel
    {
        #region Properties

        public RealtimeWire Wire
        {
            get => Holder.GetAttribute<RealtimeWire>();
            set => Holder.SetAttribute(value);
        }

        #endregion

        #region Initializers


        #endregion

        #region Methods

        protected FirebaseProperty PropertyFactory()
        {
            return new FirebaseProperty();
        }

        public void MakeRealtime(RealtimeWire wire)
        {
            wire.OnStart += delegate
            {
                Wire = wire;
                foreach (var prop in this)
                {
                    var subWire = Wire.Child(prop.Key, Wire.InvokeSetFirst);
                    prop.Value.MakeRealtime(subWire);
                    subWire.InvokeStart();
                }
            };
            wire.OnStop += delegate
            {
                Wire = null;
                foreach (var prop in this)
                {
                    prop.Value.Wire.InvokeStop();
                }
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
                        var props = new (string, StreamData)[0];

                        if (streamObject.Object is MultiStreamData multi) props = multi.Data.Select(i => (i.Key, i.Value)).ToArray();
                        else if (streamObject.Object is SingleStreamData single) props = new (string, StreamData)[] { (streamObject.Path[1], single) };
                        else if (streamObject.Object is null) props = new (string, StreamData)[0];

                        //var hasSubChanges = ReplaceProperties(props,
                        //    args =>
                        //    {
                        //        var subStreamObject = new StreamObject(args.value, args.property.Key);
                        //        return args.property.Wire.InvokeStream(subStreamObject);
                        //    });
                        //if (hasSubChanges) hasChanges = true;
                    }
                    else if (streamObject.Path.Length == 2)
                    {
                        var props = new (string, StreamData)[0];

                        if (streamObject.Object is MultiStreamData multi) props = new (string, StreamData)[] { (streamObject.Path[1], multi) };
                        else if (streamObject.Object is SingleStreamData single) props = new (string, StreamData)[] { (streamObject.Path[1], single) };
                        else if (streamObject.Object is null) props = new (string, StreamData)[] { (streamObject.Path[1], null) };

                        //var hasSubChanges = UpdateProperties(props,
                        //    args =>
                        //    {
                        //        var subStreamObject = new StreamObject(args.value, args.property.Key);
                        //        return args.property.Wire.InvokeStream(subStreamObject);
                        //    });
                        //if (hasSubChanges) hasChanges = true;
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
            var hasChanges = false;
            foreach (var prop in new Dictionary<string, FirebaseProperty>(this))
            {
                if (prop.Value.Delete()) hasChanges = true;
                Remove(prop.Key);
            }
            return hasChanges;
        }

        #endregion
    }
}
