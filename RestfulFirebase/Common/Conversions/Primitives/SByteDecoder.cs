using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class SByteDecoder : DataTypeDecoder<sbyte>
    {
        public override Decodable CreateDerived(sbyte value)
        {
            return new Decodable(value.ToString());
        }

        public override sbyte ParseValue(Decodable decodable)
        {
            if (sbyte.TryParse(decodable.Data, out sbyte result)) return result;
            throw new Exception("Parse error");
        }
    }
}
