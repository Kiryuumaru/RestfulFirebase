using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class SByteDecoder : DataTypeDecoder<sbyte>
    {
        public override ObservableProperty Parse(sbyte value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override sbyte Parse(ObservableProperty decodable)
        {
            if (sbyte.TryParse(decodable.Data, out sbyte result)) return result;
            throw new Exception("Parse error");
        }
    }
}
