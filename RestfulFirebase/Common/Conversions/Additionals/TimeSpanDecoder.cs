using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class TimeSpanDecoder : DataTypeDecoder<TimeSpan>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(TimeSpan value)
        {
            return new ObservablePropertyHolder.ObservableProperty(value.TotalHours.ToString());
        }

        public override TimeSpan ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            if (double.TryParse(decodable.Holder.Data, out double result)) return TimeSpan.FromHours(result);
            throw new Exception("Parse error");
        }
    }
}
