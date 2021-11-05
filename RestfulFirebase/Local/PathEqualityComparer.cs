using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Local
{
    internal class PathEqualityComparer : EqualityComparer<string[]>
    {
        public static PathEqualityComparer Instance { get; } = new PathEqualityComparer();

        private PathEqualityComparer()
        {

        }

        public override bool Equals(string[] x, string[] y)
        {
            if (x != null && y != null)
            {
                return Enumerable.SequenceEqual(x, y);
            }
            else if (x == null || y == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode(string[] obj)
        {
            return 467214278 + (obj == null ? 0 : EqualityComparer<string[]>.Default.GetHashCode(obj));
        }
    }
}
