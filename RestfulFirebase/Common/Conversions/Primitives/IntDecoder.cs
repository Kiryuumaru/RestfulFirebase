using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class IntDecoder : DataTypeDecoder<int>
    {
        public override Decodable CreateDerived(int value)
        {
            return new Decodable(value.ToString());
        }

        public override int ParseValue(Decodable decodable)
        {
            if (int.TryParse(decodable.Data, out int result)) return result;
            throw new Exception("Parse error");
        }
    }
}
