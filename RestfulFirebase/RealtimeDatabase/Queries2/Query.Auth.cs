using RestfulFirebase.Common.Abstractions;
using RestfulFirebase.RealtimeDatabase.References;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace RestfulFirebase.RealtimeDatabase.Queries2;

public partial class FluentQuery<TQuery>
{
    /// <summary>
    /// Sets the <see cref="IAuthorization"/> used by the request.
    /// </summary>
    /// <returns>
    /// The query with new added authorization.
    /// </returns>
    public TQuery Authorization(IAuthorization? authorization)
    {
        if (authorization == null)
        {
            return (TQuery)this;
        }

        object query = new Query(Reference, this, async ct =>
        {
            var response = await authorization.GetFreshToken(ct);
            if (response.IsError)
            {
                return response;
            }

            if (authorization.IsAccessToken)
            {
                response.Append($"access_token={response.Result}");
            }
            else
            {
                response.Append($"auth={response.Result}");
            }

            return response;
        });

        return (TQuery)query;
    }
}
