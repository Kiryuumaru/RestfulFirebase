using System;
using System.Globalization;

namespace RestfulFirebase.Database.Query
{
    public class FilterQuery : ParameterQuery 
    {
        private readonly Func<string> valueFactory;
        private readonly Func<double> doubleValueFactory;
        private readonly Func<long> longValueFactory;
        private readonly Func<bool> boolValueFactory;

        public FilterQuery(RestfulFirebaseApp app, FirebaseQuery parent, Func<string> filterFactory, Func<string> valueFactory)
            : base(app, parent, filterFactory)
        {
            this.valueFactory = valueFactory;
        }

        public FilterQuery(RestfulFirebaseApp app, FirebaseQuery parent, Func<string> filterFactory, Func<double> valueFactory)
            : base(app, parent, filterFactory)
        {
            doubleValueFactory = valueFactory;
        }

        public FilterQuery(RestfulFirebaseApp app, FirebaseQuery parent, Func<string> filterFactory, Func<long> valueFactory)
            : base(app, parent, filterFactory)
        {
            longValueFactory = valueFactory;
        }

        public FilterQuery(RestfulFirebaseApp app, FirebaseQuery parent, Func<string> filterFactory, Func<bool> valueFactory)
            : base(app, parent, filterFactory)
        {
            boolValueFactory = valueFactory;
        }

        protected override string BuildUrlParameter(FirebaseQuery child)
        {
            if (valueFactory != null)
            {
                if(valueFactory() == null)
                {
                    return $"null";
                }
                return $"\"{valueFactory()}\"";
            }
            else if (doubleValueFactory != null)
            {
                return doubleValueFactory().ToString(CultureInfo.InvariantCulture);
            }
            else if (longValueFactory != null)
            {
                return longValueFactory().ToString();
            }
            else if (boolValueFactory != null)
            {
                return $"{boolValueFactory().ToString().ToLower()}";
            }

            return string.Empty;
        }
    }
}
