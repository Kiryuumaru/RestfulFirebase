using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class CompressedDateTimeDecoder : DataTypeDecoder<CompressedDateTime>
    {
        public override string Encode(CompressedDateTime value)
        {
            return Helpers.EncodeUnixDateTime(value);
        }

        public override CompressedDateTime Decode(string data)
        {
            if (string.IsNullOrEmpty(data)) return default;
            var dateTime = Helpers.DecodeUnixDateTime(data);
            if (dateTime.HasValue) return dateTime.Value;
            throw new Exception("Parse error");
        }
    }
}
