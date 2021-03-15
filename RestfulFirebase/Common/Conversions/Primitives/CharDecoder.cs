using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class CharDecoder : DataTypeDecoder<char>
    {
        public override Decodable CreateDerived(char value)
        {
            return new Decodable(value.ToString());
        }

        public override char ParseValue(Decodable decodable)
        {
            if (char.TryParse(decodable.Data, out char result)) return result;
            throw new Exception("Parse error");
        }
    }
}
