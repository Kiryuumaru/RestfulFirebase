using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class SByteDecoder : DataTypeDecoder<sbyte>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(sbyte value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override sbyte ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (sbyte.TryParse(decodable.Holder.Data, out sbyte result)) return result;
            throw new Exception("Parse error");
        }
    }
}
