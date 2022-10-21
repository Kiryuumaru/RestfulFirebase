namespace RestfulFirebase.FirestoreDatabase.References;

public abstract partial class Reference
{
    internal static Reference Parse(Reference reference, string[] path)
    {
        ArgumentException.ThrowIfHasNullOrEmpty(path);

        Reference currentPath = reference;

        for (int i = 0; i < path.Length; i++)
        {
            if (currentPath is CollectionReference colPath)
            {
                currentPath = colPath.Document(path[i]);
            }
            else if (currentPath is DocumentReference docPath)
            {
                currentPath = docPath.Collection(path[i]);
            }
        }

        return currentPath;
    }

    internal static Reference Parse(FirebaseApp app, string[] path)
    {
        ArgumentException.ThrowIfHasNullOrEmpty(path);

        Reference currentPath = app.FirestoreDatabase.Collection(path[0]);

        for (int i = 1; i < path.Length; i++)
        {
            if (currentPath is CollectionReference colPath)
            {
                currentPath = colPath.Document(path[i]);
            }
            else if (currentPath is DocumentReference docPath)
            {
                currentPath = docPath.Collection(path[i]);
            }
        }

        return currentPath;
    }

    internal static Reference Parse(FirebaseApp app, Reference? reference, string[] path)
    {
        return reference == null ? Parse(app, path) : Parse(reference, path);
    }
}
