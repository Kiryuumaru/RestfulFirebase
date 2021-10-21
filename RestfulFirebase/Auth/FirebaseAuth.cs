using Newtonsoft.Json;

namespace RestfulFirebase.Auth
{
    /// <summary>
    /// Provides raw firebase auth JSON properties.
    /// </summary>
    public class FirebaseAuth
    {
        /// <summary>
        /// Creates new instance of <see cref="FirebaseAuth"/>
        /// </summary>
        public FirebaseAuth()
        {

        }

        /// <summary>
        /// Gets or sets the firebase token which can be used for authenticated queries. 
        /// </summary>
        [JsonProperty("idToken")]
        public string FirebaseToken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the refresh token of the underlying service which can be used to get a new access token. 
        /// </summary>
        [JsonProperty("refreshToken")]
        public string RefreshToken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the numbers of seconds since the token is created.
        /// </summary>
        [JsonProperty("expiresIn")]
        public int? ExpiresIn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public User User
        {
            get;
            set;
        }
    }
}