using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class IntDecoder : DataTypeDecoder<int>
    {
        public override string Encode(int value)
        {
            return value.ToString();
        }

        public override int Decode(string data)
        {
            if (int.TryParse(data, out int result)) return result;
            throw new Exception("Parse error");
        }
    }
}
