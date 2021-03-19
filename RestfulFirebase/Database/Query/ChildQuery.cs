using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Query
{

    public class ChildQuery : FirebaseQuery
    {
        private readonly Func<string> pathFactory;

        public ChildQuery(FirebaseQuery parent, Func<string> pathFactory, RestfulFirebaseApp app)
            : base(parent, app)
        {
            this.pathFactory = pathFactory;
        }

        public ChildQuery(RestfulFirebaseApp app, Func<string> pathFactory)
            : this(null, pathFactory, app)
        {
        }

        public ChildQuery Child(Func<string> pathFactory)
        {
            return new ChildQuery(this, pathFactory, App);
        }

        public ChildQuery Child(string path)
        {
            return Child(() => path);
        }

        public ShallowQuery Shallow()
        {
            return new ShallowQuery(this, App);
        }

        public OrderQuery OrderBy(Func<string> propertyNameFactory)
        {
            return new OrderQuery(this, propertyNameFactory, App);
        }

        public OrderQuery OrderBy(string propertyName)
        {
            return OrderBy(() => propertyName);
        }

        public OrderQuery OrderByKey()
        {
            return OrderBy("$key");
        }

        public OrderQuery OrderByValue()
        {
            return OrderBy("$value");
        }

        public OrderQuery OrderByPriority()
        {
            return OrderBy("$priority");
        }

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
