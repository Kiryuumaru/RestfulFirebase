using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class UIntDecoder : DataTypeDecoder<uint>
    {
        public override ObservableProperty Parse(uint value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override uint Parse(ObservableProperty decodable)
        {
            if (uint.TryParse(decodable.Data, out uint result)) return result;
            throw new Exception("Parse error");
        }
    }
}
