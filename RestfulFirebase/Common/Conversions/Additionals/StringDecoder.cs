using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class StringDecoder : DataTypeDecoder<string>
    {
        public override string TypeIdentifier => "string";

        protected override string EncodeValue(string value)
        {
            return value;
        }

        protected override string DecodeData(string data)
        {
            return data;
        }
    }
}
