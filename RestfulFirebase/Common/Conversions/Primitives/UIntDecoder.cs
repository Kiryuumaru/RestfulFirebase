using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class UIntDecoder : DataTypeDecoder<uint>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(uint value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override uint ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (uint.TryParse(decodable.Holder.Data, out uint result)) return result;
            throw new Exception("Parse error");
        }
    }
}
