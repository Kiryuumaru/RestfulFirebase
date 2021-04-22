using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class IntConverter : DataTypeConverter<int>
    {
        public override string Encode(int value)
        {
            return value.ToString();
        }

        public override int Decode(string data, int defaultValue = default)
        {
            if (string.IsNullOrEmpty(data)) return defaultValue;
            if (int.TryParse(data, out int result)) return result;
            return defaultValue;
        }
    }
}
