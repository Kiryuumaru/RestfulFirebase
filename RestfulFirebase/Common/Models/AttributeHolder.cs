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

        private abstract class Attribute
        {
            public string Key { get; set; }
            public string Group { get; set; }
        }

        private class Attribute<T> : Attribute
        {
            public T Value { get; set; }
        }

        #endregion

        #region Properties

        private readonly List<Attribute> attributes = new List<Attribute>();

        #endregion

        #region Initializers

        protected AttributeHolder(AttributeHolder holder)
        {
            attributes = holder == null ? new List<Attribute>() : holder.attributes;
            foreach (var property in GetType().GetProperties(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
            {
                property.GetValue(this);
            }
        }

        #endregion

        #region Methods

        protected (string Key, string Group, T Value) GetAttribute<T>(string key, string group, T defaultValue = default)
        {
            var attribute = (Attribute<T>)attributes.FirstOrDefault(i => i.Key.Equals(key) && i.Group.Equals(group));
            if (attribute == null)
            {
                attribute = new Attribute<T>()
                {
                    Key = key,
                    Group = group,
                    Value = defaultValue
                };
                attributes.Add(attribute);
            }
            return (attribute.Key, attribute.Group, attribute.Value);
        }

        protected void SetAttribute<T>(string key, string group, T value)
        {
            var attribute = (Attribute<T>)attributes.FirstOrDefault(i => i.Key.Equals(key) && i.Group.Equals(group));
            if (attribute == null)
            {
                attribute = new Attribute<T>()
                {
                    Key = key,
                    Group = group,
                    Value = value
                };
                attributes.Add(attribute);
            }
            attribute.Value = value;
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
