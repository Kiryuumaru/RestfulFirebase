using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class CharDecoder : DataTypeDecoder<char>
    {
        public override ObservableProperty Parse(char value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override char Parse(ObservableProperty decodable)
        {
            if (char.TryParse(decodable.Data, out char result)) return result;
            throw new Exception("Parse error");
        }
    }
}
