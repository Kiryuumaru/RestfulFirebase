using System;
using System.Collections.Generic;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;

namespace RestfulFirebase.Attributes
{
    /// <summary>
    /// Attribute to mark the field as a firebase property. The field must also have a <see cref="ObservablePropertyAttribute"/> attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class FirebasePropertyAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the firebase name of the property.
        /// </summary>
        public string? Name { get; set; } = null;
    }
}
