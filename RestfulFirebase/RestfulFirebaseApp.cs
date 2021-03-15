using RestfulFirebase.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase
{
    public class RestfulFirebaseApp
    {
        public FirebaseConfig Config { get; private set; }
        public FirebaseAuthApp Auth { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestfulFirebaseApp"/> class.
        /// </summary>
        /// <param name="authConfig"> The auth config. </param>
        public RestfulFirebaseApp(FirebaseConfig config)
        {
            Config = config;
            Auth = new FirebaseAuthApp(this);
        }
    }
}
