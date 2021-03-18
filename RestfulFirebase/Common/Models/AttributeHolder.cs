using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public abstract class AttributeHolder
    {
        #region Helpers

        private class Attribute
        {
            public string Key { get; set; }
            public object Value { get; set; }
            public string Group { get; set; }
        }

        #endregion

        #region Properties

        private readonly List<Attribute> attributes = new List<Attribute>();

        #endregion

        #region Initializers

        protected AttributeHolder(AttributeHolder holder)
        {
            attributes = holder == null ? new List<Attribute>() : holder.attributes;
        }

        #endregion

        #region Methods

        protected (string Key, string Group, object Value) GetAttribute(string key, string group)
        {
            var attribute = attributes.FirstOrDefault(i => i.Key.Equals(key) && i.Group.Equals(group));
            return attribute == null ? (null, null, null) : (attribute.Key, attribute.Group, attribute.Value);
        }

        protected IEnumerable<(string Key, string Group, object Value)>  GetAttributes(string group)
        {
            return attributes.Where(i => i.Group.Equals(group)).Select(i => (i.Key, i.Group, i.Value));
        }

        protected void SetAttribute(string key, string group, object value)
        {
            var attribute = attributes.FirstOrDefault(i => i.Key.Equals(key) && i.Group.Equals(group));
            if (attribute == null)
            {
                attribute = new Attribute()
                {
                    Key = key,
                    Group = group,
                    Value = value
                };
            }
            attributes.Add(attribute);
        }

        protected void DeleteAttribute(string key, string group)
        {
            var attribute = attributes.FirstOrDefault(i => i.Key.Equals(key) && i.Group.Equals(group));
            if (attribute == null) return;
            attributes.Remove(attribute);
        }

        #endregion
    }
}
