using System;

namespace RestfulFirebase.Exceptions
{
    /// <summary>
    /// Occurs when the cascade IRealtimeModel has no parameterless constructor but there`s no provided default value.
    /// </summary>
    public class DatabaseInvalidCascadeRealtimeModelException : DatabaseException
    {
        internal DatabaseInvalidCascadeRealtimeModelException()
            : this(null)
        {

        }

        internal DatabaseInvalidCascadeRealtimeModelException(Exception innerException)
            : base("Cascade IRealtimeModel with no parameterless constructor should have a default value.", innerException)
        {

        }
    }
}
