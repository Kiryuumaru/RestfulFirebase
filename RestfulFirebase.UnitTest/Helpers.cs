using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace RestfulFirebase.UnitTest;

internal class Helpers
{
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

    public static FirebaseConfig GetFirebaseConfig()
    {
        if (firebaseConfig == null)
        {
            var secrets = new ConfigurationBuilder()
                .AddUserSecrets<Helpers>()
                .Build();

            string? projectId = secrets["FIREBASE_PROJECT_ID"] ?? Environment.GetEnvironmentVariable("FIREBASE_PROJECT_ID");
            string? apiKey = secrets["FIREBASE_APIKEY"] ?? Environment.GetEnvironmentVariable("FIREBASE_APIKEY");

            Assert.NotNull(projectId);
            Assert.NotNull(apiKey);

            firebaseConfig = new(projectId, apiKey);
        }

        return firebaseConfig;
    }
}
