﻿using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class DataFactory
    {
        public Action<string> Set { get; private set; }
        public Func<string> Get { get; private set; }
        public DataFactory(Action<string> set, Func<string> get)
        {
            Set = set;
            Get = get;
        }
    }
}
