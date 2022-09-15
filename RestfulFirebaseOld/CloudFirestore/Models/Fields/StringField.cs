using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.FirestoreDatabase.Models.Fields
{
    public class StringField : Field<string>
    {
        public StringField(string? value)
        {
            Value = value;
        }
    }
}
