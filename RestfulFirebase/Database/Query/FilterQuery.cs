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

        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<string> valueFactory, RestfulFirebaseApp app)
            : base(parent, filterFactory, app)
        {
            this.valueFactory = valueFactory;
        }

        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<double> valueFactory, RestfulFirebaseApp app)
            : base(parent, filterFactory, app)
        {
            doubleValueFactory = valueFactory;
        }

        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<long> valueFactory, RestfulFirebaseApp app)
            : base(parent, filterFactory, app)
        {
            longValueFactory = valueFactory;
        }

        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<bool> valueFactory, RestfulFirebaseApp app)
            : base(parent, filterFactory, app)
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
