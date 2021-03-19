using System;

namespace RestfulFirebase.Auth
{
    public class FirebaseAuthEventArgs : EventArgs
    {
        public readonly FirebaseAuth FirebaseAuth;

        public FirebaseAuthEventArgs(FirebaseAuth auth)
        {
            FirebaseAuth = auth;
        }
    }
}
