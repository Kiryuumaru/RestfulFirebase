using RestfulFirebase.Common.Conversions;
using RestfulFirebase.Common.Conversions.Additionals;
using RestfulFirebase.Common.Conversions.Primitives;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class DistinctProperty : ObservableProperty
    {
        #region Properties

        public string Key
        {
            get => Holder.GetAttribute<string>(nameof(Key), nameof(DistinctProperty)).Value;
            private set => Holder.SetAttribute(nameof(Key), nameof(DistinctProperty), value);
        }

        #endregion

        #region Initializers

        public static DistinctProperty CreateFromKey(string key)
        {
            var obj = new DistinctProperty(Create())
            {
                Key = key
            };
            return obj;
        }

        public static DistinctProperty CreateFromKeyAndValue<T>(string key, T value)
        {
            var obj = new DistinctProperty(CreateFromValue(value))
            {
                Key = key
            };
            return obj;
        }

        public static DistinctProperty CreateFromKeyAndData(string key, string data)
        {
            var obj = new DistinctProperty(CreateFromData(data))
            {
                Key = key
            };
            return obj;
        }

        public static DistinctProperty CreateFromKeyAndBytes(string key, IEnumerable<byte> bytes)
        {
            var obj = new DistinctProperty(CreateFromBytes(bytes))
            {
                Key = key
            };
            return obj;
        }

        public DistinctProperty(IAttributed attributed)
            : base(attributed)
        {

        }

        #endregion

        #region Methods

        public bool Update(DistinctProperty cellModel)
        {
            if (cellModel.Key.Equals(Key))
            {
                Update(cellModel.Data);
                return true;
            }
            return false;
        }

        #endregion
    }
}
