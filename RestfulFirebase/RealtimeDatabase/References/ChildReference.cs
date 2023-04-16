using System;
using System.Linq;

namespace RestfulFirebase.RealtimeDatabase.References;

/// <summary>
/// The child reference of the cloud firestore.
/// </summary>
public partial class ChildReference : Reference
{
    /// <summary>
    /// Gets the name of the reference.
    /// </summary>
    public string Name { get; }

    internal ChildReference(RealtimeDatabase realtimeDatabase, Reference? parent, string segement)
        : base(realtimeDatabase, parent, segement)
    {
        Name = segement;
    }
}
