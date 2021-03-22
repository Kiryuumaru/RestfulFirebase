using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class CharDecoder : DataTypeDecoder<char>
    {
        public override string TypeIdentifier => "char";

        protected override string ParseValue(char value)
        {
            return value.ToString();
        }

        protected override char ParseData(string data)
        {
            if (char.TryParse(data, out char result)) return result;
            throw new Exception("Parse error");
        }
    }
}
