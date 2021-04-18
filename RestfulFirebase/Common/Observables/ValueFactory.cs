using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Observables
{
    public class ValueFactory
    {
        private Func<(Type type, object value, string tag), bool> set;
        private Func<(Type type, object defaultValue, string tag), object> get;

        public ValueFactory(
            Func<(Type type, object value, string tag), bool> set,
            Func<(Type type, object defaultValue, string tag), object> get)
        {
            this.set = set;
            this.get = get;
        }

        public bool Set(Type type, object value, string tag = null) => set((type, value, tag));

        public object Get(Type type, object defaultValue = default, string tag = null) => get((type, defaultValue, tag));
    }
}
