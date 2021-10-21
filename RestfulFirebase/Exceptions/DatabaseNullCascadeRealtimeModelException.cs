using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the cascade IRealtimeModel is null.
    /// </summary>
    public class DatabaseNullCascadeRealtimeModelException : DatabaseException
    {
        internal DatabaseNullCascadeRealtimeModelException()
            : this(null)
        {

        }

        internal DatabaseNullCascadeRealtimeModelException(Exception innerException)
            : base("Cascade IRealtimeModel cannot be null. Use IRealtimeModel.SetNull() instead.", innerException)
        {

        }
    }
}
