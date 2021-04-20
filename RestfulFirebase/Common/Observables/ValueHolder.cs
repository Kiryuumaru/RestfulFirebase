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
    public class ValueHolder
    {
        #region Properties

        private BlobFactory blobFactory;
        public BlobFactory BlobFactory
        {
            get
            {
                if (blobFactory == null)
                {
                    string blobHolder = null;
                    blobFactory = new BlobFactory(
                        args =>
                        {
                            var hasChanges = blobHolder != args.blob;
                            if (hasChanges) blobHolder = args.blob;
                            return hasChanges;
                        },
                        args =>
                        {
                            return blobHolder == null ? blobHolder : args.defaultBlob;
                        });
                }
                return blobFactory;
            }
            protected set => blobFactory = value;
        }

        public string Blob
        {
            get => BlobFactory.Get();
            set => BlobFactory.Set(value);
        }

        #endregion

        #region Initializers

        public ValueHolder()
        {

        }

        #endregion

        #region Methods

        public virtual bool SetNull(string tag = null)
        {
            var hasChanges = BlobFactory.Get() != null;
            BlobFactory.Set(null, tag);
            return hasChanges;
        }

        public virtual bool SetValue<T>(T value, Func<T, T, bool> comparator, string tag = null)
        {
            var converter = DataTypeConverter.GetConverter<T>();
            if (comparator != null)
            {
                var currentBlob = BlobFactory.Get(default, tag);
                var currentValue = converter.Decode(currentBlob);
                if (comparator(value, currentValue)) return false;
            }
            var newBlob = converter.Encode(value);
            return BlobFactory.Set(newBlob, tag);
        }

        public virtual T GetValue<T>(T defaultValue = default, string tag = null)
        {
            var converter = DataTypeConverter.GetConverter<T>();
            var defaultBlob = converter.Encode(defaultValue);
            return converter.Decode(BlobFactory.Get(defaultBlob, tag));
        }

        #endregion
    }
}
