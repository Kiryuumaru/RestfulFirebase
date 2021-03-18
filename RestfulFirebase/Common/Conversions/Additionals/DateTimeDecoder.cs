using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class DateTimeDecoder : DataTypeDecoder<DateTime>
    {
        public override ObservableProperty Parse(DateTime value)
        {
            return ObservableProperty.CreateFromData(Helpers.EncodeDateTime(value));
        }

        public override DateTime Parse(ObservableProperty decodable)
        {
            var dateTime = Helpers.DecodeDateTime(decodable.Data);
            if (dateTime.HasValue) return dateTime.Value;
            throw new Exception("Parse error");
        }
    }
}
