using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when a dictionary item has no parameterless constructor.
    /// </summary>
    public class DatabaseInvalidDictionaryItemConstructorException : DatabaseException
    {
        internal DatabaseInvalidDictionaryItemConstructorException()
            : this(null)
        {

        }

        internal DatabaseInvalidDictionaryItemConstructorException(Exception innerException)
            : base("Dictionary item with no parameterless constructor is not valid.", innerException)
        {

        }
    }
}
