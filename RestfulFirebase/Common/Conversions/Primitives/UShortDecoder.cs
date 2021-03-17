using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class UShortDecoder : DataTypeDecoder<ushort>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(ushort value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override ushort ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (ushort.TryParse(decodable.Holder.Data, out ushort result)) return result;
            throw new Exception("Parse error");
        }
    }
}
