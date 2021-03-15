using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ShortDecoder : DataTypeDecoder<short>
    {
        public override Decodable CreateDerived(short value)
        {
            return new Decodable(value.ToString());
        }

        public override short ParseValue(Decodable decodable)
        {
            if (short.TryParse(decodable.Data, out short result)) return result;
            throw new Exception("Parse error");
        }
    }
}
