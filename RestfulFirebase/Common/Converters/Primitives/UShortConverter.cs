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

        public override ushort Decode(string data, ushort defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (ushort.TryParse(data, out ushort result)) return result;
            return defaultValue;
        }
    }
}
