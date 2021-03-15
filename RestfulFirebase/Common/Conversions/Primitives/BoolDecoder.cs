using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class BoolDecoder : DataTypeDecoder<bool>
    {
        public override Decodable CreateDerived(bool value)
        {
            return new Decodable(value ? "1" : "0");
        }

        public override bool ParseValue(Decodable decodable)
        {
            return decodable.Data.Equals("1");
        }
    }
}
