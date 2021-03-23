using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class IntDecoder : DataTypeDecoder<int>
    {
        public override string TypeIdentifier => "int";

        protected override string EncodeValue(int value)
        {
            return value.ToString();
        }

        protected override int DecodeData(string data)
        {
            if (int.TryParse(data, out int result)) return result;
            throw new Exception("Parse error");
        }
    }
}
