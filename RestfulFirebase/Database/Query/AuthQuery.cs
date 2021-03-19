using System;

namespace RestfulFirebase.Database.Query
{
    public class AuthQuery : ParameterQuery
    {
        private readonly Func<string> tokenFactory;

        public AuthQuery(FirebaseQuery parent, Func<string> tokenFactory, RestfulFirebaseApp app) : base(parent, () => app.Config.AsAccessToken ? "access_token" : "auth", app)
        {
            this.tokenFactory = tokenFactory;
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return tokenFactory();
        }
    }
}
