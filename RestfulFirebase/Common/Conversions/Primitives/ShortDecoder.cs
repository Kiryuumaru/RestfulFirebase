using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ShortDecoder : DataTypeDecoder<short>
    {
        public override string TypeIdentifier => "short";

        protected override string ParseValue(short value)
        {
            return value.ToString();
        }

        protected override short ParseData(string data)
        {
            if (short.TryParse(data, out short result)) return result;
            throw new Exception("Parse error");
        }
    }
}
