using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class BoolDecoder : DataTypeDecoder<bool>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(bool value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value ? "1" : "0");
        }

        public override bool ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            return decodable.Holder.Data.Equals("1");
        }
    }
}
