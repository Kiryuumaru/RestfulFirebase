using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace RestfulFirebase.Auth
{
    public enum FirebaseAuthType
    {
        [EnumMember(Value = "facebook.com")]
        Facebook,

        [EnumMember(Value = "google.com")]
        Google,

        [EnumMember(Value = "github.com")]
        Github,

        [EnumMember(Value = "twitter.com")]
        Twitter,

        [EnumMember(Value = "password")]
        EmailAndPassword
    } 
}
