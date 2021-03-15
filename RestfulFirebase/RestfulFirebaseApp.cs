using RestfulFirebase.Auth;
using RestfulFirebase.Database;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase
{
    /// <summary>
    /// Firebase App which acts as an entry point to the entire rest api calls.
    /// </summary>
    public class RestfulFirebaseApp : IDisposable
    {
        public FirebaseConfig Config { get; }
        public FirebaseAuthApp Auth { get; }
        public FirebaseDatabaseApp Database { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestfulFirebaseApp"/> class.
        /// </summary>
        /// <param name="authConfig"> The auth config. </param>
        public RestfulFirebaseApp(FirebaseConfig config)
        {
            Config = config;
            Auth = new FirebaseAuthApp(this);
            Database = new FirebaseDatabaseApp(this);
        }

        public void Dispose()
        {
            Auth?.Dispose();
            Database?.Dispose();
        }
    }
}
