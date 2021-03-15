namespace RestfulFirebase.Database.Query 
{
    using System;
    using System.Globalization;

    /// <summary>
    /// Represents a firebase filtering query, e.g. "?LimitToLast=10".
    /// </summary>
    public class FilterQuery : ParameterQuery 
    {
        private readonly Func<string> valueFactory;
        private readonly Func<double> doubleValueFactory;
        private readonly Func<long> longValueFactory;
        private readonly Func<bool> boolValueFactory;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filterFactory"> The filter. </param>
        /// <param name="valueFactory"> The value for filter. </param>
        /// <param name="app"> The owner. </param>  
        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<string> valueFactory, RestfulFirebaseApp app)
            : base(parent, filterFactory, app)
        {
            this.valueFactory = valueFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filterFactory"> The filter. </param>
        /// <param name="valueFactory"> The value for filter. </param>
        /// <param name="app"> The owner. </param>
        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<double> valueFactory, RestfulFirebaseApp app)
            : base(parent, filterFactory, app)
        {
            doubleValueFactory = valueFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filterFactory"> The filter. </param>
        /// <param name="valueFactory"> The value for filter. </param>
        /// <param name="client"> The owner. </param>
        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<long> valueFactory, RestfulFirebaseApp app)
            : base(parent, filterFactory, app)
        {
            longValueFactory = valueFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterQuery"/> class.
        /// </summary>
        /// <param name="parent"> The parent. </param>
        /// <param name="filterFactory"> The filter. </param>
        /// <param name="valueFactory"> The value for filter. </param>
        /// <param name="app"> The owner. </param>
        public FilterQuery(FirebaseQuery parent, Func<string> filterFactory, Func<bool> valueFactory, RestfulFirebaseApp app)
            : base(parent, filterFactory, app)
        {
            boolValueFactory = valueFactory;
        }

        /// <summary>
        /// The build url parameter.
        /// </summary>
        /// <param name="child"> The child. </param> 
        /// <returns> Url parameter part of the resulting path. </returns> 
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
