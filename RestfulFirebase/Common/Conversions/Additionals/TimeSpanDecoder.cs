using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class TimeSpanDecoder : DataTypeDecoder<TimeSpan>
    {
        public override ObservableProperty Parse(TimeSpan value)
        {
            return ObservableProperty.CreateFromData(value.TotalHours.ToString());
        }

        public override TimeSpan Parse(ObservableProperty decodable)
        {
            if (double.TryParse(decodable.Data, out double result)) return TimeSpan.FromHours(result);
            throw new Exception("Parse error");
        }
    }
}
