namespace RestfulFirebase.Database.Query
{
    public class ShallowQuery : ParameterQuery
    {
        public ShallowQuery(RestfulFirebaseApp app, FirebaseQuery parent) 
            : base(app, parent, () => "shallow")
        {
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return "true";
        }
    }
}
