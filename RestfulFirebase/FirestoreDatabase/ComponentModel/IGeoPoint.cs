using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.FirestoreDatabase.ComponentModel;

public interface IGeoPoint
{
    double Latitude { get; set; }

    double Longitude { get; set; }
}
