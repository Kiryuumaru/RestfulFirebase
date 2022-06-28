using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.RealtimeDatabase.Attributes
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
    public sealed class FirebasePropertyAttribute : Attribute
    {
        public string? Name { get; set; } = null;
    }
}
