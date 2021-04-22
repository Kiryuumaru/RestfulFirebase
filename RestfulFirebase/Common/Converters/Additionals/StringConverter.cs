using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Converters.Additionals
{
    public class StringConverter : DataTypeConverter<string>
    {
        public override string Encode(string value)
        {
            return value;
        }

        public override string Decode(string data, string defaultValue = default)
        {
            return data;
        }
    }
}
