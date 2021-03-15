using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class UShortDecoder : DataTypeDecoder<ushort>
    {
        public override Decodable CreateDerived(ushort value)
        {
            return new Decodable(value.ToString());
        }

        public override ushort ParseValue(Decodable decodable)
        {
            if (ushort.TryParse(decodable.Data, out ushort result)) return result;
            throw new Exception("Parse error");
        }
    }
}
