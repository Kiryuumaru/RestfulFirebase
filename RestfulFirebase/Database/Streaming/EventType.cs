namespace RestfulFirebase.Database.Streaming
{
    /// <summary>
    /// The type of event. 
    /// </summary>
    public enum EventType
    {
        /// <summary>
        /// Item was inserted or updated.
        /// </summary>
        InsertOrUpdate,

        /// <summary>
        /// Item was deleted.
        /// </summary>
        Delete
    }
}
