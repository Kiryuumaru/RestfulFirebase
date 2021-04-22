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

        private string blobHolder = null;

        public string Blob
        {
            get => GetBlob();
            set => SetBlob(value);
        }

        #endregion

        #region Initializers

        public ValueHolder()
        {

        }

        #endregion

        #region Methods

        public virtual bool SetBlob(string blob, string tag = null)
        {
            lock (this)
            {
                var hasChanges = blobHolder != blob;
                if (hasChanges) blobHolder = blob;
                return hasChanges;
            }
        }

        public virtual string GetBlob(string defaultValue = null, string tag = null)
        {
            lock (this)
            {
                return blobHolder == null ? defaultValue : blobHolder;
            }
        }

        public virtual bool SetValue<T>(T value, string tag = null)
        {
            lock (this)
            {
                var deserialized = Helpers.DeserializeString(GetBlob(default, tag));
                if (deserialized == null) deserialized = new string[1];
                deserialized[0] = DataTypeConverter.GetConverter<T>().Encode(value);
                return SetBlob(Helpers.SerializeString(deserialized), tag);
            }
        }

        public virtual bool SetRawValue(string value, string tag = null)
        {
            lock (this)
            {
                var deserialized = Helpers.DeserializeString(GetBlob(default, tag));
                if (deserialized == null) deserialized = new string[1];
                deserialized[0] = value;
                return SetBlob(Helpers.SerializeString(deserialized), tag);
            }
        }

        public virtual T GetValue<T>(T defaultValue = default, string tag = null)
        {
            lock (this)
            {
                var deserialized = Helpers.DeserializeString(GetBlob(default, tag));
                if (deserialized == null) deserialized = new string[1];
                return DataTypeConverter.GetConverter<T>().Decode(deserialized[0], defaultValue);
            }
        }

        public virtual string GetRawValue(string defautlValue = default, string tag = null)
        {
            lock (this)
            {
                var deserialized = Helpers.DeserializeString(GetBlob(default, tag));
                if (deserialized == null) return defautlValue;
                return deserialized[0];
            }
        }

        public virtual T GetAdditional<T>(string key, T defaultValue = default, string tag = null)
        {
            lock (this)
            {
                var deserialized = Helpers.DeserializeString(GetBlob(default, tag));
                if (deserialized == null) deserialized = new string[1];
                var data = Helpers.BlobGetValue(deserialized.Skip(1).ToArray(), key);
                return DataTypeConverter.GetConverter<T>().Decode(data, defaultValue);
            }
        }

        public virtual bool SetAdditional<T>(string key, T value, string tag = null)
        {
            lock (this)
            {
                var deserialized = Helpers.DeserializeString(GetBlob(default, tag));
                if (deserialized == null) deserialized = new string[1];
                var data = DataTypeConverter.GetConverter<T>().Encode(value);
                var adsData = Helpers.BlobSetValue(deserialized.Skip(1).ToArray(), key, data);
                var newEncodedData = new string[adsData.Length + 1];
                newEncodedData[0] = deserialized[0];
                Array.Copy(adsData, 0, newEncodedData, 1, adsData.Length);
                return SetBlob(Helpers.SerializeString(newEncodedData), tag);
            }
        }

        public virtual bool DeleteAdditional(string key, string tag = null)
        {
            lock (this)
            {
                var deserialized = Helpers.DeserializeString(GetBlob(default, tag));
                if (deserialized == null) deserialized = new string[1];
                var adsData = Helpers.BlobDeleteValue(deserialized.Skip(1).ToArray(), key);
                var newEncodedData = new string[adsData.Length + 1];
                newEncodedData[0] = deserialized[0];
                Array.Copy(adsData, 0, newEncodedData, 1, adsData.Length);
                return SetBlob(Helpers.SerializeString(newEncodedData), tag);
            }
        }

        public virtual bool ClearAdditionals(string tag = null)
        {
            lock (this)
            {
                var deserialized = Helpers.DeserializeString(GetBlob(default, tag));
                if (deserialized == null) deserialized = new string[1];
                return SetBlob(Helpers.SerializeString(deserialized[0]), tag);
            }
        }

        #endregion
    }
}
