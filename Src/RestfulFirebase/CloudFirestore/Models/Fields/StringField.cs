using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.CloudFirestore.Models;

namespace RestfulFirebase.CloudFirestore.Models.Fields
{
    public class StringField : Field<string>
    {
        public StringField(string? value)
        {
            Value = value;
        }
    }
}
