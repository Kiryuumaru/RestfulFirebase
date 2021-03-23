﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class ByteDecoder : DataTypeDecoder<byte>
    {
        public override string TypeIdentifier => "byte";

        protected override string EncodeValue(byte value)
        {
            return value.ToString();
        }

        protected override byte DecodeData(string data)
        {
            if (byte.TryParse(data, out byte result)) return result;
            throw new Exception("Parse error");
        }
    }
}
