namespace RestfulFirebase.FirestoreDatabase.Abstractions;

/// <inheritdoc/>
public class GeoPoint : IGeoPoint
{
    /// <inheritdoc/>
    public virtual double Latitude { get; set; }

    /// <inheritdoc/>
    public virtual double Longitude { get; set; }
}