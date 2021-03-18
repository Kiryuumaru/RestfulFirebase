using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class StringDecoder : DataTypeDecoder<string>
    {
        public override ObservableProperty Parse(string value)
        {
            return ObservableProperty.CreateFromData(value);
        }

        public override string Parse(ObservableProperty decodable)
        {
            return decodable.Data;
        }
    }
}
