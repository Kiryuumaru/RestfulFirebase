using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ShortDecoder : DataTypeDecoder<short>
    {
        public override ObservableProperty Parse(short value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override short Parse(ObservableProperty decodable)
        {
            if (short.TryParse(decodable.Data, out short result)) return result;
            throw new Exception("Parse error");
        }
    }
}
