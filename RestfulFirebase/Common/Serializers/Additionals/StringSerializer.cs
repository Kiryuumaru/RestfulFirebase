﻿using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Common.Serializers.Additionals
{
    public class StringSerializer : Serializer<string>
    {
        public override string Serialize(string value)
        {
            return value;
        }

        public override string Deserialize(string data, string defaultValue = default)
        {
            return data;
        }
    }
}
