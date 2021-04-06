using RestfulFirebase.Common.Conversions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class PrimitiveData : IAttributed
    {
        #region Properties

        public AttributeHolder Holder { get; } = new AttributeHolder();

        protected DataFactory DataFactory
        {
            get
            {
                var factory = Holder.GetAttribute<DataFactory>(nameof(DataFactory), nameof(ObservableProperty)).Value;
                if (factory == null)
                {
                    factory = new DataFactory(value =>
                    {
                        Holder.SetAttribute(nameof(Data), nameof(ObservableProperty), value);
                    }, delegate
                    {
                        return Holder.GetAttribute<string>(nameof(Data), nameof(ObservableProperty)).Value;
                    });
                }
                return factory;
            }
            set => Holder.SetAttribute(nameof(DataFactory), nameof(ObservableProperty), value);
        }

        public string Data
        {
            get => DataFactory.Get.Invoke();
            private set => DataFactory.Set.Invoke(value);
        }

        #endregion

        #region Initializers

        public static ObservableProperty Create()
        {
            return new ObservableProperty(null);
        }

        public static ObservableProperty CreateFromValue<T>(T value)
        {
            var encoded = DataTypeDecoder.GetDecoder<T>().Encode(value);
            var data = Helpers.SerializeString(encoded, null);
            return CreateFromData(data);
        }

        public static ObservableProperty CreateFromData(string data)
        {
            var obj = new ObservableProperty(null);
            obj.Update(data);
            return obj;
        }

        public PrimitiveData(IAttributed attributed)
        {
            Holder.Initialize(this, attributed);
        }

        #endregion

         #region Methods

        public string GetAdditional(string key)
        {
            var deserialized = Helpers.DeserializeString(Data);
            if (deserialized == null) deserialized = new string[1];
            return Helpers.BlobGetValue(deserialized.Skip(1).ToArray(), key);
        }

        public void SetAdditional(string key, string data)
        {
            var deserialized = Helpers.DeserializeString(Data);
            if (deserialized == null) deserialized = new string[1];
            var adsData = Helpers.BlobSetValue(deserialized.Skip(1).ToArray(), key, data);
            var newEncodedData = new string[adsData.Length + 1];
            newEncodedData[0] = deserialized[0];
            Array.Copy(adsData, 0, newEncodedData, 1, adsData.Length);
            Data = Helpers.SerializeString(newEncodedData);
        }

        public void DeleteAdditional(string key)
        {
            var deserialized = Helpers.DeserializeString(Data);
            if (deserialized == null) deserialized = new string[1];
            var adsData = Helpers.BlobDeleteValue(deserialized.Skip(1).ToArray(), key);
            var newEncodedData = new string[adsData.Length + 1];
            newEncodedData[0] = deserialized[0];
            Array.Copy(adsData, 0, newEncodedData, 1, adsData.Length);
            Data = Helpers.SerializeString(newEncodedData);
        }

        public void ClearAdditionals()
        {
            var deserialized = Helpers.DeserializeString(Data);
            if (deserialized == null) deserialized = new string[1];
            Data = Helpers.SerializeString(deserialized[0]);
        }

        public void Update(string data)
        {
            Data = data;
        }

        public void Null()
        {
            Data = default;
        }

        public bool IsNull()
        {
            return Data == default;
        }

        public T ParseValue<T>()
        {
            var deserialized = Helpers.DeserializeString(Data);
            if (deserialized == null) return default;
            if (deserialized.Length == 0) return default;
            if (deserialized[0] == null) return default;
            return DataTypeDecoder.GetDecoder<T>().Decode(deserialized[0]);
        }

        #endregion
    }
}
