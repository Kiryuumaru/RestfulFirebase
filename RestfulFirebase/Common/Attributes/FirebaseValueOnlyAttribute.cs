using System;

namespace RestfulFirebase.Common.Attributes
{
    /// <summary>
    /// Attribute to mark the class to include only properties and WritableFields with <see cref="FirebaseValueAttribute"/> attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class FirebaseValueOnlyAttribute : Attribute
    {

    }
}
