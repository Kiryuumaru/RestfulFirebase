﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ULongDecoder : DataTypeDecoder<ulong>
    {
        public override string Encode(ulong value)
        {
            return value.ToString();
        }

        public override ulong Decode(string data)
        {
            if (ulong.TryParse(data, out ulong result)) return result;
            throw new Exception("Parse error");
        }
    }
}
