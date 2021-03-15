using RestfulFirebase.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase
{
    public class RestfulFirebaseApp
    {
        private readonly FirebaseConfig config;
        private readonly FirebaseAuthProvider authProvider;
        private FirebaseAuthLink authLink;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestfulFirebaseApp"/> class.
        /// </summary>
        /// <param name="authConfig"> The auth config. </param>
        public RestfulFirebaseApp(FirebaseConfig config)
        {
            this.config = config;
            authProvider = new FirebaseAuthProvider(config);
        }

        public async Task SignInWithEmailAndPasswordAsync(string email, string password, string tenantId = null)
        {
            authLink = await authProvider.SignInWithEmailAndPasswordAsync(email, password, tenantId);
        }

        public void Signout()
        {

        }
    }
}
