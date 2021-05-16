using System;
using System.Text.Json.Serialization;

namespace RestfulFirebase.Auth
{
    public class FirebaseAuth
    {
        public FirebaseAuth()
        {
            Created = DateTime.Now;
        }

        /// <summary>
        /// Gets or sets the firebase token which can be used for authenticated queries. 
        /// </summary>
        [JsonPropertyName("idToken")]
        public string FirebaseToken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the refresh token of the underlying service which can be used to get a new access token. 
        /// </summary>
        [JsonPropertyName("refreshToken")]
        public string RefreshToken
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the numbers of seconds since <see cref="Created"/> when the token expires.
        /// </summary>
        [JsonPropertyName("expiresIn")]
        public int? ExpiresIn
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets when this token was created.
        /// </summary>
        public DateTime? Created
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

        /// <summary>
        /// Specifies whether the token already expired. 
        /// </summary>
        public bool IsExpired()
        {
            if (!Created.HasValue || !ExpiresIn.HasValue) return true;
            return DateTime.Now > Created.Value.AddSeconds(ExpiresIn.Value - 10);
        }
    }
}