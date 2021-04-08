using System;

namespace RestfulFirebase.Database.Query
{
    public class AuthQuery : ParameterQuery
    {
        private readonly Func<string> tokenFactory;

        public AuthQuery(RestfulFirebaseApp app, FirebaseQuery parent, Func<string> tokenFactory)
            : base(app, parent, () => app.Config.AsAccessToken ? "access_token" : "auth")
        {
            this.tokenFactory = tokenFactory;
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return tokenFactory();
        }
    }
}
