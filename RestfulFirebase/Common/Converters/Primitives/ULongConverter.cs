using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class ULongConverter : DataTypeConverter<ulong>
    {
        public override string Encode(ulong value)
        {
            return value.ToString();
        }

        public override ulong Decode(string data, ulong defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (ulong.TryParse(data, out ulong result)) return result;
            return defaultValue;
        }
    }
}
