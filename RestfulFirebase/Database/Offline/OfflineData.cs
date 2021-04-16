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
        public SmallDateTime Modified
        {
            get => PrimitiveBlob.GetAdditional<SmallDateTime>(FirebaseProperty.ModifiedKey);
            set => PrimitiveBlob.SetAdditional(FirebaseProperty.ModifiedKey, value);
        }
        public OfflineData(PrimitiveBlob primitiveBlob)
        {
            PrimitiveBlob = primitiveBlob;
        }
    }
}
