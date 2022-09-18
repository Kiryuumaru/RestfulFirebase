﻿using System;
using System.Net;

namespace RestfulFirebase.FirestoreDatabase.Exceptions;

/// <summary>
/// Occurs when there`s an internal server error.
/// </summary>
public class FirestoreDatabaseInternalServerErrorException : FirestoreDatabaseException
{
    internal FirestoreDatabaseInternalServerErrorException(string? requestUrl, string? requestContent, string? response, HttpStatusCode httpStatusCode, Exception exception)
        : base("An internal server error occured.", requestUrl, requestContent, response, httpStatusCode, exception)
    {

    }
}
