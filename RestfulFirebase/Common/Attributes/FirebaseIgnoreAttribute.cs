using System;

namespace RestfulFirebase.Common.Attributes
{
    /// <summary>
    /// Attribute to mark the field or property to ignore from firebase value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class FirebaseIgnoreAttribute : Attribute
    {

    }
}
