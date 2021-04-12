using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class PropertyHolder
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

        public DistinctProperty Property { get; private set; }
        public string Group { get; private set; }
        public string PropertyName { get; private set; }
        public PropertyHolder(DistinctProperty property, string group, string propertyName)
        {
            Property = property;
            Group = group;
            PropertyName = propertyName;
        }
    }
}
