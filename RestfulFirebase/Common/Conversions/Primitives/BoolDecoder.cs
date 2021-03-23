using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class BoolDecoder : DataTypeDecoder<bool>
    {
        public override string TypeIdentifier => "bool";

        protected override string EncodeValue(bool value)
        {
            return value ? "1" : "0";
        }

        protected override bool DecodeData(string data)
        {
            return data.Equals("1");
        }
    }
}
