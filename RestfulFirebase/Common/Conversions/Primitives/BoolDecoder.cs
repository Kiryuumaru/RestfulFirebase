using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class BoolDecoder : DataTypeDecoder<bool>
    {
        public override ObservableProperty Parse(bool value)
        {
            return ObservableProperty.CreateFromData(value ? "1" : "0");
        }

        public override bool Parse(ObservableProperty decodable)
        {
            return decodable.Data.Equals("1");
        }
    }
}
