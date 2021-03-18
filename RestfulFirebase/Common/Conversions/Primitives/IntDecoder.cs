using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class IntDecoder : DataTypeDecoder<int>
    {
        public override ObservableProperty Parse(int value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override int Parse(ObservableProperty decodable)
        {
            if (int.TryParse(decodable.Data, out int result)) return result;
            throw new Exception("Parse error");
        }
    }
}
