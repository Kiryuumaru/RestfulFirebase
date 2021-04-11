using RestfulFirebase.Common.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class PrimitiveBlob : IAttributed
    {
        #region Properties

        public AttributeHolder Holder { get; } = new AttributeHolder();

        protected BlobFactory BlobFactory
        {
            get => Holder.GetAttribute<BlobFactory>(nameof(BlobFactory), nameof(PrimitiveBlob), new BlobFactory(
                value =>
                {
                    var oldValue = Holder.GetAttribute<string>(nameof(Blob), nameof(PrimitiveBlob)).Value;
                    Holder.SetAttribute(nameof(Blob), nameof(PrimitiveBlob), value.Value);
                    return oldValue != value.Value;
                }, delegate
                {
                    return Holder.GetAttribute<string>(nameof(Blob), nameof(PrimitiveBlob)).Value;
                })).Value;
        set => Holder.SetAttribute(nameof(BlobFactory), nameof(PrimitiveBlob), value);
        }

        public string Blob
        {
            get => BlobFactory.Get.Invoke();
        }

        #endregion

        #region Initializers

        public static PrimitiveBlob Create()
        {
            return new PrimitiveBlob(null);
        }

        public static PrimitiveBlob CreateFromValue<T>(T value)
        {
            var encoded = DataTypeConverter.GetConverter<T>().Encode(value);
            var data = Helpers.SerializeString(encoded, null);
            return CreateFromBlob(data);
        }

        public static PrimitiveBlob CreateFromBlob(string blob)
        {
            var obj = new PrimitiveBlob(null);
            obj.UpdateBlob(blob);
            return obj;
        }

        public PrimitiveBlob(IAttributed attributed)
        {
            Holder.Initialize(this, attributed);
        }

        #endregion

        #region Methods

        public T GetAdditional<T>(string key)
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) deserialized = new string[1];
            var data = Helpers.BlobGetValue(deserialized.Skip(1).ToArray(), key);
            return DataTypeConverter.GetConverter<T>().Decode(data);
        }

        public bool SetAdditional<T>(string key, T value, string tag = null)
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) deserialized = new string[1];
            var data = DataTypeConverter.GetConverter<T>().Encode(value);
            var adsData = Helpers.BlobSetValue(deserialized.Skip(1).ToArray(), key, data);
            var newEncodedData = new string[adsData.Length + 1];
            newEncodedData[0] = deserialized[0];
            Array.Copy(adsData, 0, newEncodedData, 1, adsData.Length);
            return BlobFactory.Set.Invoke((Helpers.SerializeString(newEncodedData), tag));
        }

        public bool DeleteAdditional(string key, string tag = null)
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) deserialized = new string[1];
            var adsData = Helpers.BlobDeleteValue(deserialized.Skip(1).ToArray(), key);
            var newEncodedData = new string[adsData.Length + 1];
            newEncodedData[0] = deserialized[0];
            Array.Copy(adsData, 0, newEncodedData, 1, adsData.Length);
            return BlobFactory.Set.Invoke((Helpers.SerializeString(newEncodedData), tag));
        }

        public bool ClearAdditionals(string tag = null)
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) deserialized = new string[1];
            return BlobFactory.Set.Invoke((Helpers.SerializeString(deserialized[0]), tag));
        }

        public bool UpdateBlob(string blob, string tag = null)
        {
            return BlobFactory.Set.Invoke((blob, tag));
        }

        public bool UpdateData(string data, string tag = null)
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) deserialized = new string[1];
            deserialized[0] = data;
            return BlobFactory.Set.Invoke((Helpers.SerializeString(deserialized), tag));
        }

        public string GetData()
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) return null;
            return deserialized[0];
        }

        public T ParseValue<T>()
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) return default;
            if (deserialized.Length == 0) return default;
            if (deserialized[0] == null) return default;
            return DataTypeConverter.GetConverter<T>().Decode(deserialized[0]);
        }

        #endregion
    }
}
