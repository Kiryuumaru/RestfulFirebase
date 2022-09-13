using System;
using System.Collections.Generic;
using System.Text;
using ObservableHelpers.ComponentModel;

namespace RestfulFirebase.Attributes
{
    /// <summary>
    /// Attribute to mark the class to include only properties and fields with <see cref="FirebaseValueAttribute"/> attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class FirebaseValueOnlyAttribute : Attribute
    {

    }
}
