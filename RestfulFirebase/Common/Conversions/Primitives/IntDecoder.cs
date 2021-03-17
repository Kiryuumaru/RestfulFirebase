using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class IntDecoder : DataTypeDecoder<int>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(int value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.ToString());
        }

        public override int ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (int.TryParse(decodable.Holder.Data, out int result)) return result;
            throw new Exception("Parse error");
        }
    }
}
