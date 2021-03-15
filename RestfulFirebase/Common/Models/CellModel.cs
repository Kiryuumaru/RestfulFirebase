using RestfulFirebase.Common.Conversions;
using RestfulFirebase.Common.Conversions.Additionals;
using RestfulFirebase.Common.Conversions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class CellModel : Decodable
    {
        public string Key { get; protected set; }
        public string Group { get; protected set; }

        public CellModel(string data, string key, string group = "") : base(data)
        {
            Key = key;
            Group = group;
        }

        public CellModel(IEnumerable<byte> bytes, string key, string group = "") : base(bytes)
        {
            Key = key;
            Group = group;
        }

        public bool Update(CellModel cellModel)
        {
            if (cellModel.Key.Equals(Key))
            {
                Group = cellModel.Group;
                Update(cellModel.Data);
                return true;
            }
            return false;
        }

        public static CellModel CreateDerived<T>(T value, string key)
        {
            var decodable = DataTypeDecoder.GetDecoder<T>().CreateDerived(value);
            return new CellModel(decodable.Data, key);
        }
    }
}
