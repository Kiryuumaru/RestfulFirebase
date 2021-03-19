using System;

namespace RestfulFirebase.Database.Query
{
    public class OrderQuery : ParameterQuery
    {
        private readonly Func<string> propertyNameFactory;

        public OrderQuery(ChildQuery parent, Func<string> propertyNameFactory, RestfulFirebaseApp app)
            : base(parent, () => "orderBy", app)
        {
            this.propertyNameFactory = propertyNameFactory;
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            return $"\"{propertyNameFactory()}\"";
        }
    }
}
