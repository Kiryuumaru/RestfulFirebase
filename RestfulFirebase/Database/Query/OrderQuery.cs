using System;

namespace RestfulFirebase.Database.Query
{
    public class OrderQuery : ParameterQuery
    {
        private readonly Func<string> propertyNameFactory;

        public OrderQuery(RestfulFirebaseApp app, ChildQuery parent, Func<string> propertyNameFactory)
            : base(app, parent, () => "orderBy")
        {
            this.propertyNameFactory = propertyNameFactory;
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return $"\"{propertyNameFactory()}\"";
        }
    }
}
