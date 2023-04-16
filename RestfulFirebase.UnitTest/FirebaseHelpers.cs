using Microsoft.Extensions.Configuration;
using System;
using System.Text.Json;
using Xunit;

namespace RestfulFirebase.UnitTest;

internal class FirebaseHelpers
{
    internal static string TestInstanceId = Guid.NewGuid().ToString();

    public static JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        Converters =
        {
            ModelWithCustomSerializer.Converter.Instance
        }
    };

    private static FirebaseConfig? firebaseConfig;

    public static FirebaseApp GetFirebaseApp()
    {
        if (firebaseConfig == null)
        {
            var secrets = new ConfigurationBuilder()
                .AddUserSecrets<FirebaseHelpers>()
                .Build();

            string? projectId = secrets["FIREBASE_PROJECT_ID"] ?? Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
            string? apiKey = secrets["FIREBASE_APIKEY"] ?? Environment.GetEnvironmentVariable("FIREBASE_APIKEY");

            Assert.NotNull(projectId);
            Assert.NotNull(apiKey);

            firebaseConfig = new(projectId, apiKey)
            {
                JsonSerializerOptions = JsonSerializerOptions
            };
        }

        return new(firebaseConfig);
    }
}
