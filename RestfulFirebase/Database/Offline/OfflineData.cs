﻿using RestfulFirebase.Common;
using RestfulFirebase.Common.Models;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Offline
{
    public class OfflineData : ValueHolder
    {
        public string Value => GetRawValue();

        public SmallDateTime Modified => GetAdditional<SmallDateTime>(FirebaseObject.ModifiedKey);

        public OfflineData(string blob)
        {
            Blob = blob;
        }
    }
}