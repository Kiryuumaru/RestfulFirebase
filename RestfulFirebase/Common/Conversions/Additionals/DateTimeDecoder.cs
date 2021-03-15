using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class DateTimeDecoder : DataTypeDecoder<DateTime>
    {
        public override Decodable CreateDerived(DateTime value)
        {
            return new Decodable(Helpers.EncodeDateTime(value));
        }

        public override DateTime ParseValue(Decodable decodable)
        {
            var dateTime = Helpers.DecodeDateTime(decodable.Data);
            if (dateTime.HasValue) return dateTime.Value;
            throw new Exception("Parse error");
        }
    }
}
