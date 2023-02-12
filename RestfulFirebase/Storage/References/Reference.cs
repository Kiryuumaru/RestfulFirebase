using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Text.Json;
using RestfulFirebase.Storage.Buckets;
using System.Xml.Linq;

namespace RestfulFirebase.Storage.References;

/// <summary>
/// Provides firebase storage reference node implementations.
/// </summary>
public partial class Reference
{
    /// <summary>
    /// Gets the <see cref="FirebaseApp"/> this reference uses.
    /// </summary>
    public FirebaseApp App { get; }

    /// <summary>
    /// Gets the <see cref="Buckets.Bucket"/> this reference uses.
    /// </summary>
    public Bucket Bucket { get; }

    /// <summary>
    /// Gets the parent of the query.
    /// </summary>
    public Reference? Parent { get; }

    /// <summary>
    /// Gets the name of the reference.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the path of the reference.
    /// </summary>
    public string[] Path { get; }

    internal Reference(Bucket bucket, Reference? parent, string child)
    {
        App = bucket.App;
        Bucket = bucket;

        Name = child;

        if (parent == null)
        {
            Path = new string[] { child };
        }
        else
        {
            Path = new string[parent.Path.Length + 1];
            Array.Copy(parent.Path, Path, parent.Path.Length);
            Path[^1] = child;
        }
    }
}
