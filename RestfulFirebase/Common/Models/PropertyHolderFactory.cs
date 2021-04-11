using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class PropertyHolderFactory
    {
        public PropertyHolder this[string key] { get => Get(key); }
        public Func<(PropertyHolder PropertyHolder, string Tag), bool> Set { get; private set; }
        public Func<string, PropertyHolder> Get { get; private set; }
        public PropertyHolderFactory(
            Func<(PropertyHolder PropertyHolder, string Tag), bool> set,
            Func<string, PropertyHolder> get)
        {
            Set = set;
            Get = get;
        }
    }
}
