using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class ValueTree
    {

        public ValueTree Parent { get; private set; }
        public string Key { get; private set; }
        public bool IsEndNode { get; private set; }

        public ValueTree()
        {

        }

        public ValueTree(ValueTree parent, string key)
        {
            Parent = parent;
            Key = key;
        }


        public ValueTree Child(string key)
        {
            return new ValueTree(this, key);
        }
    }
}
