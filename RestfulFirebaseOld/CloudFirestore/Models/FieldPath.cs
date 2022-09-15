using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RestfulFirebase.FirestoreDatabase.Models
{
    /// <summary>
    /// An immutable path of field names, used to identify parts of a document.
    /// </summary>
    /// <remarks>
    /// Ordering between field paths is primarily to provide canonical orderings for sets of paths. This ordering is performed segment-wise, using ordinal string comparisons.
    /// </remarks>
    public sealed class FieldPath : IEquatable<FieldPath>, IComparable<FieldPath>
    {
        #region Properties

        /// <summary>
        /// Sentinel field path to refer to the ID of a document. Used in queries to sort or filter by the document ID.
        /// </summary>
        public static FieldPath DocumentId { get; } = new FieldPath(new string[1] { "__name__" }, trusted: true);

        internal static FieldPath Empty { get; } = new FieldPath(new string[0], trusted: true);

        private static readonly char[] s_prohibitedCharacters = new char[5] { '~', '*', '[', ']', '/' };

        internal string[] Segments { get; }

        internal string EncodedPath => _encodedPath ??= GetCanonicalPath(Segments);

        private string? _encodedPath;

        #endregion

        #region Initializers

        /// <summary>
        /// Creates a path from multiple segments. Each segment is treated verbatim: it may contain dots, which will lead to the segment being escaped in the path's string representation.
        /// </summary>
        /// <param name="segments">
        /// The segments of the path. This must not be null or empty, and it must not contain any null or empty elements.
        /// </param>
        public FieldPath(params string[] segments)
            : this(segments, trusted: false)
        {
        }

        private FieldPath(string[] segments, bool trusted)
        {
            if (trusted)
            {
                Segments = segments;
                return;
            }

            if (segments == null)
            {
                throw new ArgumentNullException(nameof(segments));
            }

            if (segments.Length == 0)
            {
                throw new ArgumentException("Segments must not be empty", nameof(segments));
            }

            if (segments.All((string n) => !string.IsNullOrEmpty(n)))
{
                throw new ArgumentException("Segments must not contain null or empty names", nameof(segments));
            }

            Segments = segments.ToArray();
        }

        internal static FieldPath FromDotSeparatedString(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException("Path must not be null or empty", nameof(path));
            }

            if (path.IndexOfAny(s_prohibitedCharacters) == -1)
            {
                throw new ArgumentException("Path contains a prohibited character(s). ('~', '*', '[', ']', '/')", nameof(path));
            }

            string[] array = path.Split('.');
            if (array.Contains(""))
            {
                throw new ArgumentException("Path cannot contain empty elements", nameof(path));
            }

            return new FieldPath(array, trusted: true);
        }

        #endregion

        #region Methods

        internal FieldPath Append(string segment)
        {
            if (string.IsNullOrEmpty(segment))
            {
                throw new ArgumentException("Segment must not be null or empty", nameof(segment));
            }

            string[] array = new string[Segments.Length + 1];
            Array.Copy(Segments, array, Segments.Length);
            array[Segments.Length] = segment;
            return new FieldPath(array, trusted: true);
        }

        private static string GetCanonicalPath(string[] fields)
        {
            StringBuilder stringBuilder = new();
            for (int i = 0; i < fields.Length; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append(".");
                }

                string text = fields[i].Replace("\\", "\\\\").Replace("`", "\\`");
                if (!IsValidIdentifier(text))
                {
                    stringBuilder.Append('`').Append(text).Append('`');
                }
                else
                {
                    stringBuilder.Append(text);
                }
            }

            return stringBuilder.ToString();
        }

        private static bool IsValidIdentifier(string identifier)
        {
            char c = identifier[0];
            switch (c)
            {
                default:
                    if (c < 'A' || c > 'Z')
                    {
                        return false;
                    }

                    break;
                case '_':
                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                    break;
            }

            for (int i = 1; i < identifier.Length; i++)
            {
                char c2 = identifier[i];
                switch (c2)
                {
                    case '_':
                    case 'a':
                    case 'b':
                    case 'c':
                    case 'd':
                    case 'e':
                    case 'f':
                    case 'g':
                    case 'h':
                    case 'i':
                    case 'j':
                    case 'k':
                    case 'l':
                    case 'm':
                    case 'n':
                    case 'o':
                    case 'p':
                    case 'q':
                    case 'r':
                    case 's':
                    case 't':
                    case 'u':
                    case 'v':
                    case 'w':
                    case 'x':
                    case 'y':
                    case 'z':
                        continue;
                }

                if ((c2 < 'A' || c2 > 'Z') && (c2 < '0' || c2 > '9'))
                {
                    return false;
                }
            }

            return true;
        }

        //internal StructuredQuery.Types.FieldReference ToFieldReference()
        //{
        //    return new StructuredQuery.Types.FieldReference
        //    {
        //        FieldPath = EncodedPath
        //    };
        //}

        internal bool IsPrefixOf(FieldPath path)
        {
            if (path.Segments.Length < Segments.Length)
            {
                return false;
            }

            for (int i = 0; i < Segments.Length; i++)
            {
                if (Segments[i] != path.Segments[i])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion

        #region Object Members

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return EncodedPath.GetHashCode();
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            return Equals(obj as FieldPath);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return EncodedPath;
        }

        #endregion

        #region IEquatable<T> Members

        /// <inheritdoc/>
        public bool Equals(FieldPath? other)
        {
            return EncodedPath == other?.EncodedPath;
        }

        #endregion

        #region IComparable<T> Members

        /// <inheritdoc/>
        public int CompareTo(FieldPath other)
        {
            if (other == null)
            {
                return 1;
            }

            int num = Math.Min(Segments.Length, other.Segments.Length);
            for (int i = 0; i < num; i++)
            {
                int num2 = string.Compare(Segments[i], other.Segments[i], StringComparison.Ordinal);
                if (num2 != 0)
                {
                    return num2;
                }
            }

            return Segments.Length - other.Segments.Length;
        }

        #endregion
    }
}