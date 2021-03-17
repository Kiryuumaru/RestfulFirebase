using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class StringDecoder : DataTypeDecoder<string>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(string value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value);
        }

        public override string ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            return decodable.Holder.Data;
        }
    }
}
