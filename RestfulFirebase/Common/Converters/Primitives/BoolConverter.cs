using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Primitives
{
    public class BoolConverter : DataTypeConverter<bool>
    {
        public override string Encode(bool value)
        {
            return value ? "1" : "0";
        }

        public override bool Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            return data.Equals("1");
        }
    }
}
