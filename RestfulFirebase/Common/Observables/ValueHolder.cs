using RestfulFirebase.Common.Converters;
using RestfulFirebase.Common.Converters.Additionals;
using RestfulFirebase.Common.Converters.Primitives;
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
            : this(null)
        {

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
                return SetBlob(DataTypeConverter.GetConverter<T>().Encode(value), tag);
            }
        }

        public virtual T GetValue<T>(T defaultValue = default, string tag = null)
        {
            lock (this)
            {
                return DataTypeConverter.GetConverter<T>().Decode(GetBlob(default, tag), defaultValue);
            }
        }

        #endregion
    }
}
