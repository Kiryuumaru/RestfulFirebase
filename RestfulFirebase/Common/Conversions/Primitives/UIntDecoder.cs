using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class UIntDecoder : DataTypeDecoder<uint>
    {
        public override Decodable CreateDerived(uint value)
        {
            return new Decodable(value.ToString());
        }

        public override uint ParseValue(Decodable decodable)
        {
            if (uint.TryParse(decodable.Data, out uint result)) return result;
            throw new Exception("Parse error");
        }
    }
}
