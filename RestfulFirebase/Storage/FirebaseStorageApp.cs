using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Storage
{
    /// <summary>
    /// Firebase auth app which acts as an entry point to the storage.
    /// </summary>
    public class FirebaseStorageApp : IDisposable
    {
        /// <summary>
        /// Gets the RestfulFirebaseApp
        /// </summary>
        public RestfulFirebaseApp App { get; }

        internal FirebaseStorageApp(RestfulFirebaseApp app)
        {
            App = app;
        }

        /// <summary>
        /// Constructs firebase path to the file.
        /// </summary>
        /// <param name="childRoot"> Root name of the entity. This can be folder or a file name or full path.</param>
        /// <example>
        ///     storage
        ///         .Child("some")
        ///         .Child("path")
        ///         .Child("to/file.png");
        /// </example>
        /// <returns> <see cref="FirebaseStorageReference"/> for fluid syntax. </returns>
        public FirebaseStorageReference Child(string childRoot)
        {
            return new FirebaseStorageReference(App, childRoot);
        }

        public void Dispose()
        {

        }
    }
}
