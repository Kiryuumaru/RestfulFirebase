namespace RestfulFirebase.Database.Query
{
    public class ShallowQuery : ParameterQuery
    {
        public ShallowQuery(FirebaseQuery parent, RestfulFirebaseApp app) 
            : base(parent, () => "shallow", app)
        {
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return "true";
        }
    }
}
