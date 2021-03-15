using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class TimeSpanDecoder : DataTypeDecoder<TimeSpan>
    {
        public override Decodable CreateDerived(TimeSpan value)
        {
            return new Decodable(value.TotalHours.ToString());
        }

        public override TimeSpan ParseValue(Decodable decodable)
        {
            if (double.TryParse(decodable.Data, out double result)) return TimeSpan.FromHours(result);
            throw new Exception("Parse error");
        }
    }
}
