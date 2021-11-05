using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the provided model is not valid.
    /// </summary>
    public class DatabaseInvalidModel : DatabaseException
    {
        internal DatabaseInvalidModel()
            : base("The provided model is not valid")
        {

        }
    }
}
