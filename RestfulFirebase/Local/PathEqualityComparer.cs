using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Local;

internal class PathEqualityComparer : EqualityComparer<string[]>
{
    public static PathEqualityComparer Instance { get; } = new PathEqualityComparer();

    private PathEqualityComparer()
    {

    }

    public override bool Equals(string[]? x, string[]? y)
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
        if (obj == null)
        {
            return 0;
        }

        return (obj as IStructuralEquatable).GetHashCode(EqualityComparer<string>.Default);
    }
}
