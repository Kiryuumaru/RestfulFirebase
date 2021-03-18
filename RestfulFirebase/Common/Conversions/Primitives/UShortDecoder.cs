using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class UShortDecoder : DataTypeDecoder<ushort>
    {
        public override ObservableProperty Parse(ushort value)
        {
            return ObservableProperty.CreateFromData(value.ToString());
        }

        public override ushort Parse(ObservableProperty decodable)
        {
            if (ushort.TryParse(decodable.Data, out ushort result)) return result;
            throw new Exception("Parse error");
        }
    }
}
