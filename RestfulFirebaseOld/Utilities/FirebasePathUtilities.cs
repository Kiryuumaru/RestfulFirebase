using RestfulFirebase.Exceptions;
using System.Linq;

namespace RestfulFirebase.Utilities;

internal static class FirebasePathUtilities
{
    internal static void EnsureValidPath(string[] path)
    {
        if (path == null || path.Length == 0)
        {
            return;
        }
        foreach (string node in path)
        {
            if (string.IsNullOrEmpty(node))
            {
                throw StringNullOrEmptyException.FromEnumerableArgument(nameof(path));
            }
            else if (node.Any(
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

    internal static void EnsureValidAndNonEmptyPath(string[] path)
    {
        if (path == null || path.Length == 0)
        {
            throw StringNullOrEmptyException.FromSingleArgument(nameof(path));
        }
        EnsureValidPath(path);
    }
}
