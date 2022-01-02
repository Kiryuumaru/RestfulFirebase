using System;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Query
{
    internal class AuthQuery : ParameterQuery
    {
        #region Properties

        private readonly Func<Task<string>> tokenFactory;

        #endregion

        #region Initializers

        internal AuthQuery(RestfulFirebaseApp app, FirebaseQuery parent, Func<Task<string>> tokenFactory)
            : base(app, parent, () => app.Config.CachedAsAccessToken ? "access_token" : "auth")
        {
            this.tokenFactory = tokenFactory;
        }

        #endregion

        #region Methods


        #endregion

        #region ParameterQuery Members

        protected override string BuildUrlParameter()
        {
            return BuildUrlParameterAsync().Result;
        }

        protected override async Task<string> BuildUrlParameterAsync()
        {
            return await tokenFactory();
        }

        #endregion
    }
}
