namespace RestfulFirebase.Database.Query
{
    /// <summary>
    /// Appends print=silent to the url.
    /// </summary>
    public class SilentQuery : ParameterQuery
    {
        internal SilentQuery(RestfulFirebaseApp app, FirebaseQuery parent) 
            : base(app, parent, () => "print")
        {
        }

        /// <inheritdoc/>
        protected override string BuildUrlParameter()
        {
            return "silent";
        }
    }
}
