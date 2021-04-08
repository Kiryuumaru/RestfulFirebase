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

        public override int Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            if (int.TryParse(data, out int result)) return result;
            throw new Exception("Parse error");
        }
    }
}
