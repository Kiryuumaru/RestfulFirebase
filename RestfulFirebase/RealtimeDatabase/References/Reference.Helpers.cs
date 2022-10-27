using System.Linq;

namespace RestfulFirebase.RealtimeDatabase.References;

public partial class Reference
{
    internal static Reference Create(RealtimeDatabase realtimeDatabase, Reference? parent, string path)
    {
        ArgumentNullException.ThrowIfNull(path);
        ArgumentException.ThrowIfEmpty(path);

        if (path.Any(
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
            ArgumentException.Throw($"\"{nameof(path)}\" contains an invalid character.");
        }

        path = path.Trim().Trim('/');

        return new Reference(realtimeDatabase, parent, path);
    }

    internal static Reference Parse(Reference reference, string[] path)
    {
        ArgumentException.ThrowIfHasNullOrEmpty(path);

        Reference currentPath = reference;

        for (int i = 0; i < path.Length; i++)
        {
            currentPath = currentPath.Child(path[i]);
        }

        return currentPath;
    }

    internal static Reference Parse(RealtimeDatabase realtimeDatabase, string[] path)
    {
        ArgumentException.ThrowIfHasNullOrEmpty(path);

        Reference currentPath = realtimeDatabase.Child(path[0]);

        for (int i = 1; i < path.Length; i++)
        {
            currentPath = currentPath.Child(path[i]);
        }

        return currentPath;
    }

    internal static Reference Parse(RealtimeDatabase realtimeDatabase, Reference? reference, string[] path)
    {
        return reference == null ? Parse(realtimeDatabase, path) : Parse(reference, path);
    }
}
