using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class CharDecoder : DataTypeDecoder<char>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(char value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override char ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (char.TryParse(decodable.Holder.Data, out char result)) return result;
            throw new Exception("Parse error");
        }
    }
}
