namespace RestfulFirebase.Database.Query
{
    /// <summary>
    /// Appends print=silent to the url.
    /// </summary>
    public class SilentQuery : ParameterQuery
    {
        public SilentQuery(FirebaseQuery parent, RestfulFirebaseApp app) 
            : base(parent, () => "print", app)
        {
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return "silent";
        }
    }
}
