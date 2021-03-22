using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class StringDecoder : DataTypeDecoder<string>
    {
        public override string TypeIdentifier => "string";

        protected override string ParseValue(string value)
        {
            return value;
        }

        protected override string ParseData(string data)
        {
            return data;
        }
    }
}
