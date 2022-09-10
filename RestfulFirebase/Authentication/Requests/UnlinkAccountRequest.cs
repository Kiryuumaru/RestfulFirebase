﻿using RestfulFirebase.Authentication.Enums;
using RestfulFirebase.Common.Requests;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace RestfulFirebase.Authentication.Requests;

/// <summary>
/// Request to unlink the account with oauth provided with auth type.
/// </summary>
public class UnlinkAccountRequest : AuthenticatedCommonRequest
{
    /// <summary>
    /// Gets or sets the <see cref="FirebaseAuthType"/> to unlink.
    /// </summary>
    public FirebaseAuthType? AuthType { get; set; }
}
