using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class StringDecoder : DataTypeDecoder<string>
    {
        public override Decodable CreateDerived(string value)
        {
            return new Decodable(value);
        }

        public override string ParseValue(Decodable decodable)
        {
            return decodable.Data;
        }
    }
}
