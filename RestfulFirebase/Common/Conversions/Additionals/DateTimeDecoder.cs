using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class DateTimeDecoder : DataTypeDecoder<DateTime>
    {
        public override ObservablePropertyHolder.ObservableProperty CreateDerived(DateTime value)
        {
            return new ObservablePropertyHolder.ObservableProperty(Helpers.EncodeDateTime(value));
        }

        public override DateTime ParseValue(ObservablePropertyHolder.ObservableProperty decodable)
        {
            var dateTime = Helpers.DecodeDateTime(decodable.Holder.Data);
            if (dateTime.HasValue) return dateTime.Value;
            throw new Exception("Parse error");
        }
    }
}
