using System.Text.Json;
using RestfulFirebase.Common.Utilities;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using RestfulFirebase.Common.Models;

namespace RestfulFirebase.Authentication;

/// <summary>
/// Provides firebase authentication implementations.
/// </summary>
public partial class AuthenticationApi
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> used.
    /// </summary>
    public FirebaseApp App { get; }

    internal AuthenticationApi(FirebaseApp app)
    {
        App = app;
    }
}
