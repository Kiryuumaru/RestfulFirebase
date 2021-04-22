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

        public virtual T GetAdditional<T>(string key, T defaultValue = default, string tag = null)
        {
            lock(BlobFactory)
            {
                var deserialized = Helpers.DeserializeString(BlobFactory.Get(default, tag));
                if (deserialized == null) deserialized = new string[1];
                var data = Helpers.BlobGetValue(deserialized.Skip(1).ToArray(), key);
                return DataTypeConverter.GetConverter<T>().Decode(data, defaultValue);
            }
        }

        public virtual bool SetAdditional<T>(string key, T value, string tag = null)
        {
            lock (BlobFactory)
            {
                var deserialized = Helpers.DeserializeString(BlobFactory.Get(default, tag));
                if (deserialized == null) deserialized = new string[1];
                var data = DataTypeConverter.GetConverter<T>().Encode(value);
                var adsData = Helpers.BlobSetValue(deserialized.Skip(1).ToArray(), key, data);
                var newEncodedData = new string[adsData.Length + 1];
                newEncodedData[0] = deserialized[0];
                Array.Copy(adsData, 0, newEncodedData, 1, adsData.Length);
                return BlobFactory.Set(Helpers.SerializeString(newEncodedData), tag);
            }
        }

        public virtual bool DeleteAdditional(string key, string tag = null)
        {
            lock (BlobFactory)
            {
                var deserialized = Helpers.DeserializeString(BlobFactory.Get(default, tag));
                if (deserialized == null) deserialized = new string[1];
                var adsData = Helpers.BlobDeleteValue(deserialized.Skip(1).ToArray(), key);
                var newEncodedData = new string[adsData.Length + 1];
                newEncodedData[0] = deserialized[0];
                Array.Copy(adsData, 0, newEncodedData, 1, adsData.Length);
                return BlobFactory.Set(Helpers.SerializeString(newEncodedData), tag);
            }
        }

        public virtual bool ClearAdditionals(string tag = null)
        {
            lock (BlobFactory)
            {
                var deserialized = Helpers.DeserializeString(BlobFactory.Get(default, tag));
                if (deserialized == null) deserialized = new string[1];
                return BlobFactory.Set(Helpers.SerializeString(deserialized[0]), tag);
            }
        }

        public virtual bool SetNull(string tag = null)
        {
            lock (BlobFactory)
            {
                var hasChanges = BlobFactory.Get(default, tag) != null;
                BlobFactory.Set(null, tag);
                return hasChanges;
            }
        }

        public virtual bool SetValueNull(string tag = null)
        {
            lock (BlobFactory)
            {
                var deserialized = Helpers.DeserializeString(BlobFactory.Get(default, tag));
                if (deserialized == null) deserialized = new string[1];
                var hasChanges = deserialized[0] != null;
                if (hasChanges)
                {
                    deserialized[0] = null;
                    BlobFactory.Set(Helpers.SerializeString(deserialized), tag);
                }
                return hasChanges;
            }
        }

        public virtual bool SetValue<T>(T value, string tag = null)
        {
            lock (BlobFactory)
            {
                var deserialized = Helpers.DeserializeString(BlobFactory.Get(default, tag));
                if (deserialized == null) deserialized = new string[1];
                deserialized[0] = DataTypeConverter.GetConverter<T>().Encode(value);
                return BlobFactory.Set(Helpers.SerializeString(deserialized), tag);
            }
        }

        public virtual bool SetBlob(string blob, string tag = null)
        {
            lock (BlobFactory)
            {
                return BlobFactory.Set(blob, tag);
            }
        }

        public virtual T GetValue<T>(T defaultValue = default, string tag = null)
        {
            lock (BlobFactory)
            {
                var deserialized = Helpers.DeserializeString(BlobFactory.Get(default, tag));
                if (deserialized == null) deserialized = new string[1];
                return DataTypeConverter.GetConverter<T>().Decode(deserialized[0], defaultValue);
            }
        }

        public virtual string GetValue(string tag = null)
        {
            lock (BlobFactory)
            {
                var deserialized = Helpers.DeserializeString(BlobFactory.Get(default, tag));
                if (deserialized == null) deserialized = new string[1];
                return deserialized[0];
            }
        }

        #endregion
    }
}
