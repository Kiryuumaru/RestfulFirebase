using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.FirestoreDatabase.Models.Fields
{
    public class IntegerField : Field<int>
    {
        public IntegerField(string? value)
        {
            if (int.TryParse(value, out int v))
            {
                Value = v;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
