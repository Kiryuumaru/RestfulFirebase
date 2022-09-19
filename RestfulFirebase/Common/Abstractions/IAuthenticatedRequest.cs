using RestfulFirebase.Authentication.Models;

namespace RestfulFirebase.Common.Abstractions;

/// <summary>
/// The base request for all authenticated firebase request.
/// </summary>
public interface IAuthenticatedRequest<T> : IRequest
    where T : IAuthorization
{
    /// <summary>
    /// Gets or sets the <typeparamref name="T"/> to authenticate the request.
    /// </summary>
    public T? Authorization { get; set; }
}
