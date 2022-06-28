using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.CloudFirestore.Models;

namespace RestfulFirebase.CloudFirestore.Models.Fields
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
