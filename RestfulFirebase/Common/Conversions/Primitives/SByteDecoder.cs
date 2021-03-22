﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Conversions.Primitives
{
    public class SByteDecoder : DataTypeDecoder<sbyte>
    {
        public override string TypeIdentifier => "sbyte";

        protected override string ParseValue(sbyte value)
        {
            return value.ToString();
        }

        protected override sbyte ParseData(string data)
        {
            if (sbyte.TryParse(data, out sbyte result)) return result;
            throw new Exception("Parse error");
        }
    }
}
