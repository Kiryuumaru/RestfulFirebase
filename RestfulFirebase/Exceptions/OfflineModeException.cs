using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the operation is not executed because the offline mode is enabled.
    /// </summary>
    public class OfflineModeException : Exception
    {
        internal OfflineModeException()
            : this(null)
        {

        }

        internal OfflineModeException(Exception innerException)
            : base("The operation is not executed because the offline mode is enabled.", innerException)
        {

        }
    }
}
