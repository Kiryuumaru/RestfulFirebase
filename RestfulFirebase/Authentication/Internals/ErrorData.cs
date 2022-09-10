using System;
using System.Text.Json.Serialization;

namespace RestfulFirebase.Authentication.Internals;

internal class ErrorData
{
    public Error? Error { get; set; }
}

internal class Error
{
    public int Code { get; set; }

    public string? Message { get; set; }
}
