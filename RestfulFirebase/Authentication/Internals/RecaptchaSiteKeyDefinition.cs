using System;
using System.Text.Json.Serialization;

namespace RestfulFirebase.Authentication.Internals;

internal class RecaptchaSiteKeyDefinition
{
    public string? RecaptchaSiteKey { get; set; }
}
