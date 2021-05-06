using RestfulFirebase.Common.Serializers;
using RestfulFirebase.Common.Serializers.Additionals;
using RestfulFirebase.Common.Serializers.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Observables
{
    public class ValueHolder : IAttributed
    {
        #region Properties

        public AttributeHolder Holder { get; } = new AttributeHolder();

        private string BlobHolder
        {
            get => Holder.GetAttribute<string>();
            set => Holder.SetAttribute(value);
        }

        public string Blob
        {
            get => GetBlob();
            set => SetBlob(value);
        }


        #endregion

        #region Initializers

        public ValueHolder(IAttributed attributed)
        {
            Holder.Inherit(attributed);
        }

        public ValueHolder()
        {
            Holder.Inherit(null);
        }

        #endregion

        #region Methods

        public virtual bool SetBlob(string blob, string tag = null)
        {
            lock (this)
            {
                var hasChanges = BlobHolder != blob;
                if (hasChanges) BlobHolder = blob;
                return hasChanges;
            }
        }

        public virtual string GetBlob(string defaultValue = null, string tag = null)
        {
            lock (this)
            {
                return BlobHolder == null ? defaultValue : BlobHolder;
            }
        }

        public virtual bool SetValue<T>(T value, string tag = null)
        {
            lock (this)
            {
                return SetBlob(Serializer.Serialize(value), tag);
            }
        }

        public virtual T GetValue<T>(T defaultValue = default, string tag = null)
        {
            lock (this)
            {
                return Serializer.Deserialize(GetBlob(default, tag), defaultValue);
            }
        }

        #endregion
    }
}
