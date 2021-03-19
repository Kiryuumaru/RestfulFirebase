namespace RestfulFirebase.Database.Query
{
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
