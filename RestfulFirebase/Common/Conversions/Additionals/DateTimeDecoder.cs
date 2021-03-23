using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class DateTimeDecoder : DataTypeDecoder<DateTime>
    {
        public override string TypeIdentifier => "dateTime";

        protected override string EncodeValue(DateTime value)
        {
            return Helpers.EncodeDateTime(value);
        }

        protected override DateTime DecodeData(string data)
        {
            var dateTime = Helpers.DecodeDateTime(data);
            if (dateTime.HasValue) return dateTime.Value;
            throw new Exception("Parse error");
        }
    }
}
