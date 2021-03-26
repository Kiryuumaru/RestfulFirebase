﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Additionals
{
    public class StringDecoder : DataTypeDecoder<string>
    {
        public override string Encode(string value)
        {
            return value;
        }

        public override string Decode(string data)
        {
            return data;
        }
    }
}
