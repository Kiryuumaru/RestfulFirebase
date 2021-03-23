using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class UShortDecoder : DataTypeDecoder<ushort>
    {
        public override string TypeIdentifier => "ushort";

        protected override string EncodeValue(ushort value)
        {
            return value.ToString();
        }

        protected override ushort DecodeData(string data)
        {
            if (ushort.TryParse(data, out ushort result)) return result;
            throw new Exception("Parse error");
        }
    }
}
