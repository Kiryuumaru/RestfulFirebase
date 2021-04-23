using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Observables
{
    public interface IAttributed
    {
        AttributeHolder Holder { get; }
    }

    public class AttributeHolder
    {
        #region Helpers

        private abstract class Attribute
        {
            public string Key { get; set; }
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

        public void Inherit(IAttributed attributed)
        {
            if (attributed != null) attributes = attributed.Holder.attributes;
        }

        #endregion

        #region Methods

        public T GetAttribute<T>(T defaultValue = default, [CallerMemberName] string key = null)
        {
            var attribute = (Attribute<T>)attributes.FirstOrDefault(i => i.Key.Equals(key));
            if (attribute == null)
            {
                attribute = new Attribute<T>()
                {
                    Key = key,
                    Value = defaultValue
                };
                attributes.Add(attribute);
            }
            return attribute.Value;
        }

        public void SetAttribute<T>(T value, [CallerMemberName] string key = null)
        {
            var attribute = (Attribute<T>)attributes.FirstOrDefault(i => i.Key.Equals(key));
            if (attribute == null)
            {
                attribute = new Attribute<T>()
                {
                    Key = key,
                    Value = value
                };
                attributes.Add(attribute);
            }
            attribute.Value = value;
        }

        public void DeleteAttribute(string key)
        {
            var attribute = attributes.FirstOrDefault(i => i.Key.Equals(key));
            if (attribute == null) return;
            attributes.Remove(attribute);
        }

        #endregion
    }
}
