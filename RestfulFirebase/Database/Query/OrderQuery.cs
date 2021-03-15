namespace RestfulFirebase.Database.Query
{
    using System;

    /// <summary>
    /// Represents a firebase ordering query, e.g. "?OrderBy=Foo".
    /// </summary>
    public class OrderQuery : ParameterQuery
    {
        private readonly Func<string> propertyNameFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrderQuery"/> class.
        /// </summary>
        /// <param name="parent"> The query parent. </param>
        /// <param name="propertyNameFactory"> The property name. </param>
        /// <param name="app"> The owner. </param>
        public OrderQuery(ChildQuery parent, Func<string> propertyNameFactory, RestfulFirebaseApp app)
            : base(parent, () => "orderBy", app)
        {
            this.propertyNameFactory = propertyNameFactory;
        }

        /// <summary>
        /// The build url parameter.
        /// </summary>
        /// <param name="child"> The child. </param>
        /// <returns> The <see cref="string"/>. </returns>
        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return $"\"{propertyNameFactory()}\"";
        }
    }
}
