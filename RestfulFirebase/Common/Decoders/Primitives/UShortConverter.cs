using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class UShortConverter : DataTypeConverter<ushort>
    {
        public override string Encode(ushort value)
        {
            return value.ToString();
        }

        public override ushort Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            if (ushort.TryParse(data, out ushort result)) return result;
            throw new Exception("Parse error");
        }
    }
}
