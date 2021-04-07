using RestfulFirebase.Common.Conversions;
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

        public BlobFactory BlobFactory
        {
            get
            {
                var factory = Holder.GetAttribute<BlobFactory>(nameof(BlobFactory), nameof(PrimitiveBlob)).Value;
                if (factory == null)
                {
                    factory = new BlobFactory(value =>
                    {
                        Holder.SetAttribute(nameof(Blob), nameof(PrimitiveBlob), value.Value);
                    }, delegate
                    {
                        return Holder.GetAttribute<string>(nameof(Blob), nameof(PrimitiveBlob)).Value;
                    });
                }
                return factory;
            }
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
            var encoded = DataTypeDecoder.GetDecoder<T>().Encode(value);
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
            return DataTypeDecoder.GetDecoder<T>().Decode(data);
        }

        public void SetAdditional<T>(string key, T value, string tag = null)
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) deserialized = new string[1];
            var data = DataTypeDecoder.GetDecoder<T>().Encode(value);
            var adsData = Helpers.BlobSetValue(deserialized.Skip(1).ToArray(), key, data);
            var newEncodedData = new string[adsData.Length + 1];
            newEncodedData[0] = deserialized[0];
            Array.Copy(adsData, 0, newEncodedData, 1, adsData.Length);
            BlobFactory.Set.Invoke((Helpers.SerializeString(newEncodedData), tag));
        }

        public void DeleteAdditional(string key, string tag = null)
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) deserialized = new string[1];
            var adsData = Helpers.BlobDeleteValue(deserialized.Skip(1).ToArray(), key);
            var newEncodedData = new string[adsData.Length + 1];
            newEncodedData[0] = deserialized[0];
            Array.Copy(adsData, 0, newEncodedData, 1, adsData.Length);
            BlobFactory.Set.Invoke((Helpers.SerializeString(newEncodedData), tag));
        }

        public void ClearAdditionals(string tag = null)
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) deserialized = new string[1];
            BlobFactory.Set.Invoke((Helpers.SerializeString(deserialized[0]), tag));
        }

        public string GetData()
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) return null;
            return deserialized[0];
        }

        public void UpdateBlob(string blob, string tag = null)
        {
            BlobFactory.Set.Invoke((blob, tag));
        }

        public void UpdateData(string data, string tag = null)
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) deserialized = new string[1];
            deserialized[0] = data;
            BlobFactory.Set.Invoke((Helpers.SerializeString(deserialized), tag));
        }

        public T ParseValue<T>()
        {
            var deserialized = Helpers.DeserializeString(Blob);
            if (deserialized == null) return default;
            if (deserialized.Length == 0) return default;
            if (deserialized[0] == null) return default;
            return DataTypeDecoder.GetDecoder<T>().Decode(deserialized[0]);
        }

        #endregion
    }
}
