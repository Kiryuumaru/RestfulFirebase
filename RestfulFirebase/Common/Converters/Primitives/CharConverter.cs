using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class CharConverter : DataTypeConverter<char>
    {
        public override string Encode(char value)
        {
            return value.ToString();
        }

        public override char Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            if (char.TryParse(data, out char result)) return result;
            throw new Exception("Parse error");
        }
    }
}
