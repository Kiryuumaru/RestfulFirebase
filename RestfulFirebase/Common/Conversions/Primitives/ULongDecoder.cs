using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ULongDecoder : DataTypeDecoder<ulong>
    {
        public override Decodable CreateDerived(ulong value)
        {
            return new Decodable(value.ToString());
        }

        public override ulong ParseValue(Decodable decodable)
        {
            if (ulong.TryParse(decodable.Data, out ulong result)) return result;
            throw new Exception("Parse error");
        }
    }
}
