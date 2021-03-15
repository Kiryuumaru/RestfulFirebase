namespace RestfulFirebase.Database
{
    /// <summary>
    /// Holds the object of type <typeparamref name="T" /> along with its key. 
    /// </summary>
    /// <typeparam name="T"> Type of the underlying object. </typeparam> 
    public class FirebaseObject<T> 
    {
        internal FirebaseObject(string key, T obj)
        {
            Key = key;
            Object = obj;
        }

        /// <summary>
        /// Gets the key of <see cref="Object"/>.
        /// </summary>
        public string Key
        {
            get;
        }

        /// <summary>
        /// Gets the underlying object.
        /// </summary>
        public T Object
        {
            get;
        }
    }
}
