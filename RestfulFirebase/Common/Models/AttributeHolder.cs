using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public class AttributeHolder
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

        private List<Attribute> attributes = new List<Attribute>();

        #endregion

        #region Initializers

        public void Initialize(IAttributed attributed, IAttributed derived)
        {
            if (attributed == null) throw new Exception("Attributed class is null");
            attributes = derived == null ? new List<Attribute>() : derived.Holder.attributes;
        }

        #endregion

        #region Methods

        public (string Key, string Group, T Value) GetAttribute<T>(string key, string group, T defaultValue = default)
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

        public void SetAttribute<T>(string key, string group, T value)
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

        public void DeleteAttribute(string key, string group)
        {
            attributes.RemoveAll(i => i.Key.Equals(key) && i.Group.Equals(group));
        }

        #endregion
    }
}
