using RestfulFirebase.Common.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to update the accounts profile provided with display name and photo URL.
/// </summary>
public class UpdateProfileRequest : AuthenticatedCommonRequest
{
    /// <summary>
    /// Gets or sets the new display name of the account.
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the new photo url of the account.
    /// </summary>
    public string? PhotoUrl { get; set; }
}
