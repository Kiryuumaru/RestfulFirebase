using System;

namespace RestfulFirebase.Database.Query
{
    /// <summary>
    /// Represents a firebase ordering query, e.g. "?OrderBy=Foo".
    /// </summary>
    public class OrderQuery : ParameterQuery
    {
        private readonly Func<string> propertyNameFactory;

        internal OrderQuery(RestfulFirebaseApp app, ChildQuery parent, Func<string> propertyNameFactory)
            : base(app, parent, () => "orderBy")
        {
            this.propertyNameFactory = propertyNameFactory;
        }

        /// <inheritdoc/>
        protected override string BuildUrlParameter()
        {
            return $"\"{propertyNameFactory()}\"";
        }
    }
}
