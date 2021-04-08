namespace RestfulFirebase.Database.Query
{
    public class SilentQuery : ParameterQuery
    {
        public SilentQuery(RestfulFirebaseApp app, FirebaseQuery parent) 
            : base(app, parent, () => "print")
        {
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return "silent";
        }
    }
}
