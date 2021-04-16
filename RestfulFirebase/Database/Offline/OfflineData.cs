using RestfulFirebase.Common.Models;
using RestfulFirebase.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineData
    {
        public PrimitiveBlob PrimitiveBlob { get; }
        public SmallDateTime Modified { get => PrimitiveBlob.GetAdditional<SmallDateTime>(FirebaseProperty.ModifiedKey); }
        public OfflineData(PrimitiveBlob primitiveBlob)
        {
            PrimitiveBlob = primitiveBlob;
        }
    }
}
