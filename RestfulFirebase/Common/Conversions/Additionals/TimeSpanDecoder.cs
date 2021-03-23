using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class TimeSpanDecoder : DataTypeDecoder<TimeSpan>
    {
        public override string TypeIdentifier => "timeSpan";

        protected override string EncodeValue(TimeSpan value)
        {
            return value.TotalHours.ToString();
        }

        protected override TimeSpan DecodeData(string data)
        {
            if (double.TryParse(data, out double result)) return TimeSpan.FromHours(result);
            throw new Exception("Parse error");
        }
    }
}
