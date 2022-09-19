using System;

namespace RestfulFirebase.Common.Attributes
{
    /// <summary>
    /// Attribute to mark the field or property as a firebase value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class FirebaseValueAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the firebase name of the field or property.
        /// </summary>
        public string? Name { get; set; } = null;
    }
}
