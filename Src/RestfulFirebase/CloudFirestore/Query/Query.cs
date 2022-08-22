//using RestfulFirebase.CloudFirestore.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Net.Http.Headers;
//using System.Text;
//using System.Threading.Tasks;

//namespace RestfulFirebase.CloudFirestore.Query;

///// <summary>
///// The base reference of the cloud firestore.
///// </summary>
//public class Query
//{
//    #region Properties

//    /// <summary>
//    /// Gets the <see cref="RestfulFirebaseApp"/> used by this instance.
//    /// </summary>
//    public RestfulFirebaseApp App { get => _root.App; }

//    /// <summary>
//    /// Gets the <see cref="FirestoreDatabase"/> used by this instance.
//    /// </summary>
//    public FirestoreDatabase Database { get => _root.Database; }

//    /// <summary>
//    /// Gets or sets <c>true</c> whether to use authenticated requests; otherwise <c>false</c>.
//    /// </summary>
//    public bool AuthenticateRequests { get; set; } = true;

//    private readonly int _offset;

//    private readonly (int count, LimitType type)? _limit;

//    private readonly IReadOnlyList<InternalOrdering> _orderings;

//    private readonly IReadOnlyList<InternalFilter>? _filters;

//    private readonly IReadOnlyList<FieldPath>? _projections;

//    private readonly Cursor _startAt;

//    private readonly Cursor _endAt;

//    private readonly QueryRoot _root;

//    private bool IsLimitToLast
//    {
//        get
//        {
//            ref readonly (int count, LimitType type)? limit = ref _limit;
//            if (!limit.HasValue)
//            {
//                return false;
//            }

//            return limit.GetValueOrDefault().type == LimitType.Last;
//        }
//    }

//    internal string ParentPath => _root.ParentPath;

//    #endregion

//    #region Initializers

//    private Query(QueryRoot root)
//    {
//        _root = root;
//        _orderings = new List<InternalOrdering>();
//    }

//    private protected Query(FirestoreDatabase database, DocumentReference parent, string collectionId)
//        : this(QueryRoot.ForCollection(database, parent, collectionId))
//    {
//    }

//    private Query(QueryRoot root, int offset, (int count, LimitType type)? limit, IReadOnlyList<InternalOrdering> orderings, IReadOnlyList<InternalFilter>? filters, IReadOnlyList<FieldPath>? projections, Cursor startAt, Cursor endAt)
//    {
//        _root = root;
//        _offset = offset;
//        _limit = limit;
//        _orderings = orderings;
//        _filters = filters;
//        _projections = projections;
//        _startAt = startAt;
//        _endAt = endAt;
//    }

//    #endregion

//    #region Methods

//    /// <summary>
//    /// Specifies the field paths to return in the results.
//    /// </summary>
//    /// <param name="fieldPaths">The dot-separated field paths to select. Must not be null or empty, or contain null or empty elements.
//    /// </param>
//    /// <returns>
//    /// A new query based on the current one, but with the specified projection applied.
//    /// </returns>
//    /// <exception cref="ArgumentNullException">
//    /// This call replaces any previously-specified projections in the query.
//    /// </exception>
//    public Query Select(params string[] fieldPaths)
//    {
//        if (fieldPaths == null)
//        {
//            throw new ArgumentNullException(nameof(fieldPaths));
//        }

//        FieldPath[] fieldPaths2 = fieldPaths.Select(FieldPath.FromDotSeparatedString).ToArray();
//        return Select(fieldPaths2);
//    }

//    /// <summary>
//    /// Specifies the field paths to return in the results.
//    /// </summary>
//    /// <remarks>
//    /// This call replaces any previously-specified projections in the query.
//    /// </remarks>
//    /// <param name="fieldPaths">
//    /// The field paths to select. Must not be null or contain null elements. If this is empty, the document ID is implicitly selected.
//    /// </param>
//    /// <returns>
//    /// A new query based on the current one, but with the specified projection applied.
//    /// </returns>
//    public Query Select(params FieldPath[] fieldPaths)
//    {
//        if (fieldPaths == null)
//        {
//            throw new ArgumentNullException(nameof(fieldPaths));
//        }

//        if (fieldPaths.Contains(null))
//        {
//            throw new ArgumentException("Field paths must not contain a null element", nameof(fieldPaths));
//        }

//        if (fieldPaths.Length == 0)
//        {
//            fieldPaths = new FieldPath[1] { FieldPath.DocumentId };
//        }

//        return new Query(_root, _offset, _limit, _orderings, _filters, new List<FieldPath>(fieldPaths), _startAt, _endAt);
//    }

//    internal async Task<HttpClient> GetClient()
//    {
//        var client = App.Config.HttpClientFactory.GetHttpClient();

//        if (AuthenticateRequests && App.Auth.Session != null)
//        {
//            string token = await App.Auth.Session.GetFreshToken();
//            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
//        }

//        return client;
//    }

//    internal abstract string BuildUrl();

//    internal abstract string BuildUrlSegment();

//    #endregion

//    #region Helpers

//    private struct InternalOrdering : IEquatable<InternalOrdering>
//    {
//        internal FieldPath Field { get; }

//        internal StructuredQuery.Types.Direction Direction { get; }

//        internal StructuredQuery.Types.Direction InverseDirection => Direction switch
//        {
//            StructuredQuery.Types.Direction.Ascending => StructuredQuery.Types.Direction.Descending,
//            StructuredQuery.Types.Direction.Descending => StructuredQuery.Types.Direction.Ascending,
//            _ => throw new InvalidOperationException($"Can't invert direction {Direction}"),
//        };

//        internal StructuredQuery.Types.Order ToProto(bool invertDirection)
//        {
//            return new StructuredQuery.Types.Order
//            {
//                Direction = (invertDirection ? InverseDirection : Direction),
//                Field = Field.ToFieldReference()
//            };
//        }

//        public override int GetHashCode()
//        {
//            return GaxEqualityHelpers.CombineHashCodes(Field.GetHashCode(), (int)Direction);
//        }

//        internal InternalOrdering(FieldPath field, StructuredQuery.Types.Direction direction)
//        {
//            Field = field;
//            Direction = direction;
//        }

//        public override bool Equals(object obj)
//        {
//            if (obj is InternalOrdering)
//            {
//                InternalOrdering other = (InternalOrdering)obj;
//                return Equals(other);
//            }

//            return false;
//        }

//        public bool Equals(InternalOrdering other)
//        {
//            if (Field.Equals(other.Field))
//            {
//                return Direction == other.Direction;
//            }

//            return false;
//        }
//    }

//    private struct InternalFilter : IEquatable<InternalFilter>
//    {
//        private readonly int _op;

//        private readonly Value _value;

//        internal FieldPath Field { get; }

//        internal StructuredQuery.Types.Filter ToProto()
//        {
//            if (_value != null)
//            {
//                return new StructuredQuery.Types.Filter
//                {
//                    FieldFilter = new StructuredQuery.Types.FieldFilter
//                    {
//                        Field = Field.ToFieldReference(),
//                        Op = (StructuredQuery.Types.FieldFilter.Types.Operator)_op,
//                        Value = _value
//                    }
//                };
//            }

//            return new StructuredQuery.Types.Filter
//            {
//                UnaryFilter = new StructuredQuery.Types.UnaryFilter
//                {
//                    Field = Field.ToFieldReference(),
//                    Op = (StructuredQuery.Types.UnaryFilter.Types.Operator)_op
//                }
//            };
//        }

//        private InternalFilter(FieldPath field, int op, Value value)
//        {
//            Field = field;
//            _op = op;
//            _value = value;
//        }

//        //
//        // Summary:
//        //     Checks whether this is a comparison operator.
//        internal bool IsOrderingFilter()
//        {
//            if ((_value == null || _op != 3) && _op != 4 && _op != 1)
//            {
//                return _op == 2;
//            }

//            return true;
//        }

//        internal static InternalFilter Create(SerializationContext context, FieldPath fieldPath, StructuredQuery.Types.FieldFilter.Types.Operator op, object value)
//        {
//            GaxPreconditions.CheckNotNull(fieldPath, "fieldPath");
//            StructuredQuery.Types.UnaryFilter.Types.Operator unaryOperator = GetUnaryOperator(value, op);
//            if (unaryOperator != 0)
//            {
//                return new InternalFilter(fieldPath, (int)unaryOperator, null);
//            }

//            Value value2 = ValueSerializer.Serialize(context, value);
//            ValidateNoSentinelsRecursively(value2, "Sentinel values cannot be specified in filters");
//            return new InternalFilter(fieldPath, (int)op, value2);
//        }

//        private static StructuredQuery.Types.UnaryFilter.Types.Operator GetUnaryOperator(object value, StructuredQuery.Types.FieldFilter.Types.Operator op)
//        {
//            if (value != null)
//            {
//                if (value is double)
//                {
//                    double d = (double)value;
//                    if (double.IsNaN(d))
//                    {
//                        goto IL_005b;
//                    }
//                }
//                else if (value is float)
//                {
//                    float f = (float)value;
//                    if (float.IsNaN(f))
//                    {
//                        goto IL_005b;
//                    }
//                }

//                return StructuredQuery.Types.UnaryFilter.Types.Operator.Unspecified;
//            }

//            return op switch
//            {
//                StructuredQuery.Types.FieldFilter.Types.Operator.Equal => StructuredQuery.Types.UnaryFilter.Types.Operator.IsNull,
//                StructuredQuery.Types.FieldFilter.Types.Operator.NotEqual => StructuredQuery.Types.UnaryFilter.Types.Operator.IsNotNull,
//                _ => throw new ArgumentException("Null values can only be used with the Equal/NotEqual operators", "value"),
//            };
//        IL_005b:
//            return op switch
//            {
//                StructuredQuery.Types.FieldFilter.Types.Operator.Equal => StructuredQuery.Types.UnaryFilter.Types.Operator.IsNan,
//                StructuredQuery.Types.FieldFilter.Types.Operator.NotEqual => StructuredQuery.Types.UnaryFilter.Types.Operator.IsNotNan,
//                _ => throw new ArgumentException("Not-a-number values can only be used with the Equal/NotEqual operators", "value"),
//            };
//        }

//        public override bool Equals(object obj)
//        {
//            if (obj is InternalFilter)
//            {
//                InternalFilter other = (InternalFilter)obj;
//                return Equals(other);
//            }

//            return false;
//        }

//        public bool Equals(InternalFilter other)
//        {
//            if (Field.Equals(other.Field) && _op == other._op)
//            {
//                return object.Equals(_value, other._value);
//            }

//            return false;
//        }

//        public override int GetHashCode()
//        {
//            return GaxEqualityHelpers.CombineHashCodes(Field.GetHashCode(), _op, _value?.GetHashCode() ?? (-1));
//        }
//    }

//    private sealed class DocumentSnapshotComparer : IComparer<DocumentSnapshot>
//    {
//        private readonly CloudFirestore.Query _query;

//        internal DocumentSnapshotComparer(CloudFirestore.Query query)
//        {
//            _query = query;
//        }

//        public int Compare(DocumentSnapshot x, DocumentSnapshot y)
//        {
//            GaxPreconditions.CheckArgument(x.Exists, "x", "Document snapshot comparer for a query cannot be used with snapshots of missing documents");
//            GaxPreconditions.CheckArgument(y.Exists, "y", "Document snapshot comparer for a query cannot be used with snapshots of missing documents");
//            StructuredQuery.Types.Direction direction = StructuredQuery.Types.Direction.Ascending;
//            foreach (InternalOrdering ordering in _query._orderings)
//            {
//                direction = ordering.Direction;
//                int num;
//                if (object.Equals(ordering.Field, FieldPath.DocumentId))
//                {
//                    num = x.Reference.CompareTo(y.Reference);
//                }
//                else
//                {
//                    Value value = x.ExtractValue(ordering.Field);
//                    Value value2 = y.ExtractValue(ordering.Field);
//                    if (value == null || value2 == null)
//                    {
//                        throw new InvalidOperationException("Can only compare fields that exist in the DocumentSnapshot. Please include the fields you are ordering on in your Select() call.");
//                    }

//                    num = ValueComparer.Instance.Compare(value, value2);
//                }

//                if (num != 0)
//                {
//                    return (direction == StructuredQuery.Types.Direction.Ascending) ? num : (-Math.Sign(num));
//                }
//            }

//            int num2 = x.Reference.CompareTo(y.Reference);
//            if (direction == StructuredQuery.Types.Direction.Descending)
//            {
//                num2 = -Math.Sign(num2);
//            }

//            return num2;
//        }
//    }

//    private sealed class QueryRoot : IEquatable<QueryRoot>
//    {
//        internal RestfulFirebaseApp App { get; }

//        internal FirestoreDatabase Database { get; }

//        internal string ParentPath { get; }

//        internal string CollectionId { get; }

//        internal bool AllDescendants { get; }

//        private QueryRoot(FirestoreDatabase database, string parentPath, string collectionId, bool allDescendants)
//        {
//            if (database == null)
//            {
//                throw new ArgumentNullException(nameof(database));
//            }

//            if (database == null)
//            {
//                throw new ArgumentNullException(nameof(collectionId));
//            }

//            App = database.App;
//            Database = database;
//            ParentPath = parentPath;
//            CollectionId = collectionId;
//            AllDescendants = allDescendants;
//        }

//        internal static QueryRoot ForCollection(FirestoreDatabase database, DocumentReference parent, string collectionId)
//        {
//            return new QueryRoot(database, parent?.Path ?? database?.DocumentsPath, collectionId, allDescendants: false);
//        }

//        internal static QueryRoot ForCollectionGroup(FirestoreDatabase database, string collectionId)
//        {
//            return new QueryRoot(database, database?.DocumentsPath, collectionId, allDescendants: true);
//        }

//        public override bool Equals(object obj)
//        {
//            return Equals(obj as QueryRoot);
//        }

//        public bool Equals(QueryRoot other)
//        {
//            if (other != null && Database.Equals(other.Database) && ParentPath == other.ParentPath && CollectionId == other.CollectionId)
//            {
//                return AllDescendants == other.AllDescendants;
//            }

//            return false;
//        }

//        public override int GetHashCode()
//        {
//            return GaxEqualityHelpers.CombineHashCodes(Database.GetHashCode(), ParentPath.GetHashCode(), CollectionId?.GetHashCode() ?? 0, AllDescendants ? 1 : 0);
//        }
//    }

//    private enum LimitType
//    {
//        First,
//        Last
//    }

//    #endregion
//}
