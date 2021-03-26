using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class BoolDecoder : DataTypeDecoder<bool>
    {
        public override string Encode(bool value)
        {
            return value ? "1" : "0";
        }

        public override bool Decode(string data)
        {
            return data.Equals("1");
        }
    }
}
