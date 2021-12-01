using RestfulFirebase.Exceptions;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Query
{
    /// <summary>
    /// Firebase query which references the child of current node.
    /// </summary>
    public class ChildQuery : FirebaseQuery
    {
        #region Properties

        private readonly Func<string> pathFactory;

        #endregion

        #region Initializers

        internal ChildQuery(RestfulFirebaseApp app, FirebaseQuery parent, Func<string> pathFactory)
            : base(app, parent)
        {
            this.pathFactory = pathFactory;
            if (parent != null)
            {
                if (pathFactory().Any(
                    c =>
                    {
                        switch (c)
                        {
                            case '$': return true;
                            case '#': return true;
                            case '[': return true;
                            case ']': return true;
                            case '.': return true;
                            default:
                                if ((c >= 0 && c <= 31) || c == 127)
                                {
                                    return true;
                                }
                                return false;
                        }
                    }))
                {
                    throw new DatabaseForbiddenNodeNameCharacter();
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Creates a shallow query that appends shallow=true to the url parameters. This cannot be used with any other filtering parameters.
        /// See https://firebase.google.com/docs/database/rest/retrieve-data
        /// </summary>
        /// <returns>
        /// The created <see cref="ShallowQuery"/>.
        /// </returns>
        public ShallowQuery Shallow()
        {
            return new ShallowQuery(App, this);
        }

        /// <summary>
        /// Order data by given <paramref name="propertyNameFactory"/>. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="propertyNameFactory">
        /// The property name factory.
        /// </param>
        /// <returns>
        /// The created <see cref="OrderQuery"/>.
        /// </returns>
        public OrderQuery OrderBy(Func<string> propertyNameFactory)
        {
            return new OrderQuery(App, this, propertyNameFactory);
        }

        /// <summary>
        /// Order data by given <paramref name="propertyName"/>. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        /// <returns>
        /// The created <see cref="OrderQuery"/>.
        /// </returns>
        public OrderQuery OrderBy(string propertyName)
        {
            return OrderBy(() => propertyName);
        }

        /// <summary>
        /// Order data by $key. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <returns>
        /// The created <see cref="OrderQuery"/>.
        /// </returns>
        public OrderQuery OrderByKey()
        {
            return OrderBy("$key");
        }

        /// <summary>
        /// Order data by $value. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <returns>
        /// The created <see cref="OrderQuery"/>.
        /// </returns>
        public OrderQuery OrderByValue()
        {
            return OrderBy("$value");
        }

        /// <summary>
        /// Order data by $priority. Note that this is used mainly for following filtering queries and due to firebase implementation
        /// the data may actually not be ordered.
        /// </summary>
        /// <returns>
        /// The created <see cref="OrderQuery"/>.
        /// </returns>
        public OrderQuery OrderByPriority()
        {
            return OrderBy("$priority");
        }

        #endregion

        #region ParameterQuery Members

        /// <inheritdoc/>
        protected override string BuildUrlSegment(IFirebaseQuery child)
        {
            var s = pathFactory();

            if (string.IsNullOrEmpty(s))
            {
                throw new ArgumentNullException("path");
            }
            if (s.Any(
                c =>
                {
                    switch (c)
                    {
                        case '$': return true;
                        case '[': return true;
                        case ']': return true;
                        case '#': return true;
                        case '.': 
                            if (Parent != null)
                            {
                                return true;
                            }
                            return false;
                        default:
                            if ((c >= 0 && c <= 31) || c == 127)
                            {
                                return true;
                            }
                            return false;
                    }
                }))
            {
                throw new DatabaseForbiddenNodeNameCharacter();
            }

            if (child is ChildQuery)
            {
                if (s != string.Empty && !s.EndsWith("/"))
                {
                    s += '/';
                }
            }
            else
            {
                if (s != string.Empty && s.EndsWith("/"))
                {
                    s = s.Substring(0, s.Length - 1);
                }
                s += ".json";
            }
            return s;
        }

        /// <inheritdoc/>
        protected override Task<string> BuildUrlSegmentAsync(IFirebaseQuery child)
        {
            return Task.FromResult(BuildUrlSegment(child));
        }

        #endregion
    }
}
