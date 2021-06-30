using System;

namespace RestfulFirebase.Database.Query
{
    /// <summary>
    /// Represents an auth parameter in firebase query, e.g. "?auth=xyz".
    /// </summary>
    public class AuthQuery : ParameterQuery
    {
        private readonly Func<string> tokenFactory;

        internal AuthQuery(RestfulFirebaseApp app, FirebaseQuery parent, Func<string> tokenFactory)
            : base(app, parent, () => app.Config.AsAccessToken ? "access_token" : "auth")
        {
            this.tokenFactory = tokenFactory;
        }

        /// <inheritdoc/>
        protected override string BuildUrlParameter()
        {
            return tokenFactory();
        }
    }
}
