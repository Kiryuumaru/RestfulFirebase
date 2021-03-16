namespace RestfulFirebase.Database.Query
{
    using System;

    /// <summary>
    /// Represents an auth parameter in firebase query, e.g. "?auth=xyz".
    /// </summary>
    public class AuthQuery : ParameterQuery
    {
        private readonly Func<string> tokenFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent.  </param>  
        /// <param name="tokenFactory"> The authentication token factory. </param>
        /// <param name="app"> The owner. </param>
        public AuthQuery(FirebaseQuery parent, Func<string> tokenFactory, RestfulFirebaseApp app) : base(parent, () => app.Config.AsAccessToken ? "access_token" : "auth", app)
        {
            this.tokenFactory = tokenFactory;
        }

        /// <summary>
        /// Build the url parameter value of this child. 
        /// </summary>
        /// <param name="child"> The child of this child. </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return tokenFactory();
        }
    }
}
