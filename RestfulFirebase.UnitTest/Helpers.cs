using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.UnitTest;

internal class Helpers
{
    private static FirebaseConfig? firebaseConfig;

    public static FirebaseConfig GetFirebaseConfig()
    {
        if (firebaseConfig == null)
        {
            var secrets = new ConfigurationBuilder()
                .AddUserSecrets<Helpers>()
                .Build();

            string projectId = secrets["FIREBASE_PROJECT_ID"];
            string apiKey = secrets["FIREBASE_APIKEY"];

            firebaseConfig = new(projectId, apiKey);
        }

        return firebaseConfig;
    }
}
