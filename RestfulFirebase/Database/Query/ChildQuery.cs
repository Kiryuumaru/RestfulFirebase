namespace RestfulFirebase.Database.Query
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    /// <summary>
    /// Firebase query which references the child of current node.
    /// </summary>
    public class ChildQuery : FirebaseQuery
    {
        private readonly Func<string> pathFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent.  </param>
        /// <param name="pathFactory"> The path to the child node.  </param>
        /// <param name="app"> The owner. </param>
        public ChildQuery(FirebaseQuery parent, Func<string> pathFactory, RestfulFirebaseApp app)
            : base(parent, app)
        {
            this.pathFactory = pathFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildQuery"/> class.
        /// </summary>
        /// <param name="app"> The client. </param>
        /// <param name="pathFactory"> The path to the child node.  </param>
        public ChildQuery(RestfulFirebaseApp app, Func<string> pathFactory)
            : this(null, pathFactory, app)
        {
        }

        /// <summary>
        /// References a sub child of the existing node.
        /// </summary>
        /// <param name="node"> The child. </param>
        /// <param name="pathFactory"> The path of sub child. </param>
        /// <returns> The <see cref="ChildQuery"/>. </returns>
        public ChildQuery Child(Func<string> pathFactory)
        {
            return new ChildQuery(this, pathFactory, App);
        }

        /// <summary>
        /// References a sub child of the existing node.
        /// </summary>
        /// <param name="path"> The path of sub child. </param>
        /// <returns> The <see cref="ChildQuery"/>. </returns>
        public ChildQuery Child(string path)
        {
            return Child(() => path);
        }

        /// <summary>
        /// Appends shallow=true to the url parameters. This cannot be used with any other filtering parameters.
        /// See https://firebase.google.com/docs/database/rest/retrieve-data
        /// </summary>
        /// <param name="node"> The child. </param>
        /// <returns> The <see cref="ShallowQuery"/>. </returns>
        public ShallowQuery Shallow()
        {
            return new ShallowQuery(this, App);
        }

        /// <summary>
        /// Order data by given <paramref name="propertyNameFactory"/>. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <param name="propertyNameFactory"> The property name. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public OrderQuery OrderBy(Func<string> propertyNameFactory)
        {
            return new OrderQuery(this, propertyNameFactory, App);
        }

        /// <summary>
        /// Order data by given <paramref name="propertyName"/>. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <param name="propertyName"> The property name. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public OrderQuery OrderBy(string propertyName)
        {
            return OrderBy(() => propertyName);
        }

        /// <summary>
        /// Order data by $key. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public OrderQuery OrderByKey()
        {
            return OrderBy("$key");
        }

        /// <summary>
        /// Order data by $value. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public OrderQuery OrderByValue()
        {
            return OrderBy("$value");
        }

        /// <summary>
        /// Order data by $priority. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="OrderQuery"/>. </returns>
        public OrderQuery OrderByPriority()
        {
            return OrderBy("$priority");
        }

        /// <summary>
        /// Build the url segment of this child.
        /// </summary>
        /// <param name="child"> The child of this child. </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected override string BuildUrlSegment(FirebaseQuery child)
        {
            var s = pathFactory();

            if (s != string.Empty && !s.EndsWith("/"))
            {
                s += '/';
            }

            if (!(child is ChildQuery))
            {
                return s + ".json";
            }

            return s;
        }
    }
}
