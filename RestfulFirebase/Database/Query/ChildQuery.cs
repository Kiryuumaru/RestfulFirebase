using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Query
{

    public class ChildQuery : FirebaseQuery
    {
        private readonly Func<string> pathFactory;

        public ChildQuery(RestfulFirebaseApp app, FirebaseQuery parent, Func<string> pathFactory)
            : base(app, parent)
        {
            this.pathFactory = pathFactory;
        }

        public ChildQuery(RestfulFirebaseApp app, Func<string> pathFactory)
            : this(app, null, pathFactory)
        {
        }

        public ShallowQuery Shallow()
        {
            return new ShallowQuery(App, this);
        }

        public OrderQuery OrderBy(Func<string> propertyNameFactory)
        {
            return new OrderQuery(App, this, propertyNameFactory);
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

            if (!(child is ChildQuery))
            {
                if (s != string.Empty && s.EndsWith("/"))
                {
                    s = s.Substring(0, s.Length - 1);
                }
                s += ".json";
            }
            else
            {
                if (s != string.Empty && !s.EndsWith("/"))
                {
                    s += '/';
                }
            }
            return s;
        }
    }
}
