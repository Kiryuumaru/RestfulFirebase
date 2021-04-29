using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Local
{
    public interface ILocalDatabase
    {
        bool ContainsKey(string key);
        IEnumerable<string> KeysStartsWith(string key);
        string Get(string key);
        void Set(string key, string value);
        void Delete(string key);
    }
}
