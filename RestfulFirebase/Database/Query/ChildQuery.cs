namespace RestfulFirebase.Database.Query
{
    using System;
    
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
        /// <param name="databaseApp"> The client. </param>
        /// <param name="pathFactory"> The path to the child node.  </param>
        public ChildQuery(RestfulFirebaseApp app, Func<string> pathFactory)
            : this(null, pathFactory, app)
        {
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
