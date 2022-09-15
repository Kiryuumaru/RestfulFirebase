using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.FirestoreDatabase.Models;

namespace RestfulFirebase.FirestoreDatabase.Models.Fields
{
    public class DoubleField : Field<double>
    {
        public DoubleField(string? value)
        {
            if (double.TryParse(value, out double v))
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
