using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Query
{
    /// <summary>
    /// Represents an auth parameter in firebase query, e.g. "?auth=xyz".
    /// </summary>
    public class AuthQuery : ParameterQuery
    {
        private readonly Func<Task<string>> tokenFactory;

        internal AuthQuery(RestfulFirebaseApp app, FirebaseQuery parent, Func<Task<string>> tokenFactory)
            : base(app, parent, () => app.Config.AsAccessToken ? "access_token" : "auth")
        {
            this.tokenFactory = tokenFactory;
        }

        /// <inheritdoc/>
        protected override string BuildUrlParameter()
        {
            return BuildUrlParameterAsync().Result;
        }

        /// <inheritdoc/>
        protected override async Task<string> BuildUrlParameterAsync()
        {
            return await tokenFactory();
        }
    }
}
