using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class LongDecoder : DataTypeDecoder<long>
    {
        public override string TypeIdentifier => "long";

        protected override string EncodeValue(long value)
        {
            return value.ToString();
        }

        protected override long DecodeData(string data)
        {
            if (long.TryParse(data, out long result)) return result;
            throw new Exception("Parse error");
        }
    }
}
