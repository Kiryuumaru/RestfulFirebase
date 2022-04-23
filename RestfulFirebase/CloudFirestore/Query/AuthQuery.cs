namespace RestfulFirebase.CloudFirestore.Query;

using RestfulFirebase.Exceptions;
using System;
using System.Threading.Tasks;

internal class AuthQuery : ParameterQuery
{
    #region Properties



    #endregion

    #region Initializers

    internal AuthQuery(RestfulFirebaseApp app, FirebaseQuery parent)
        : base(app, parent, () => app.Config.CachedAsAccessToken ? "access_token" : "auth")
    {

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
        if (App.Auth.Session != null)
        {
            return await App.Auth.Session.GetFreshToken();
        }

        throw new AuthNotAuthenticatedException();
    }

    #endregion
}
