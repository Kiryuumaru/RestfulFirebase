namespace RestfulFirebase.RealtimeDatabase.Query;

using RestfulFirebase.Exceptions;
using System.Threading.Tasks;

internal class AuthQuery : ParameterQuery
{
    #region Properties



    #endregion

    #region Initializers

    internal AuthQuery(RealtimeDatabase realtimeDatabase, FirebaseQuery parent)
        : base(realtimeDatabase, parent, () => realtimeDatabase.App.Config.AsAccessToken ? "access_token" : "auth")
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
