using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.FirestoreDatabase.Abstraction;

/// <summary>
/// A geo point value representing a point on the surface of Earth. Interface for firebase geoPointValue type.
/// </summary>
public interface IGeoPoint
{
    /// <summary>
    /// Gets or sets the latitude point value representing a point on the surface of Earth.
    /// </summary>
    double Latitude { get; set; }

    /// <summary>
    /// Gets or sets the longitude point value representing a point on the surface of Earth.
    /// </summary>
    double Longitude { get; set; }
}
