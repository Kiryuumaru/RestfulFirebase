using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class ShortConverter : DataTypeConverter<short>
    {
        public override string Encode(short value)
        {
            return value.ToString();
        }

        public override short Decode(string data, short defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (short.TryParse(data, out short result)) return result;
            return defaultValue;
        }
    }
}
