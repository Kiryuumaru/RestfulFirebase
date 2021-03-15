using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class LongDecoder : DataTypeDecoder<long>
    {
        public override Decodable CreateDerived(long value)
        {
            return new Decodable(value.ToString());
        }

        public override long ParseValue(Decodable decodable)
        {
            if (long.TryParse(decodable.Data, out long result)) return result;
            throw new Exception("Parse error");
        }
    }
}
