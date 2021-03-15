namespace RestfulFirebase.Auth
{
    /// <summary>
    /// The auth config. 
    /// </summary>
    public class FirebaseConfig
    {
        /// <summary>
        /// Gets or sets the api key of your Firebase app.
        /// </summary>
        public string ApiKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the database URL of your Firebase app.
        /// </summary>
        public string DatabaseURL
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the storage bucket of your Firebase app.
        /// </summary>
        public string StorageBucket
        {
            get;
            set;
        }
    }
}
