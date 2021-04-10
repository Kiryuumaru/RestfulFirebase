using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class PropertyHolder
    {
        public DistinctProperty Property { get; set; }
        public string Group { get; set; }
        public string PropertyName { get; set; }
        public PropertyHolder(DistinctProperty property, string group, string propertyName)
        {
            Property = property;
            Group = group;
            PropertyName = propertyName;
        }
    }
}
