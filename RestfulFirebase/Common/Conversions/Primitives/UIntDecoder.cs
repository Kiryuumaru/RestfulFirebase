﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class UIntDecoder : DataTypeDecoder<uint>
    {
        public override string TypeIdentifier => "uint";

        protected override string EncodeValue(uint value)
        {
            return value.ToString();
        }

        protected override uint DecodeData(string data)
        {
            if (uint.TryParse(data, out uint result)) return result;
            throw new Exception("Parse error");
        }
    }
}
