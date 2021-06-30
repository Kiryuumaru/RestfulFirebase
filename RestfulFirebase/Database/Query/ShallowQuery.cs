namespace RestfulFirebase.Database.Query
{
    /// <summary>
    /// Appends shallow=true to the url.
    /// </summary>
    public class ShallowQuery : ParameterQuery
    {
        internal ShallowQuery(RestfulFirebaseApp app, FirebaseQuery parent) 
            : base(app, parent, () => "shallow")
        {
        }

        /// <inheritdoc/>
        protected override string BuildUrlParameter()
        {
            return "true";
        }
    }
}
