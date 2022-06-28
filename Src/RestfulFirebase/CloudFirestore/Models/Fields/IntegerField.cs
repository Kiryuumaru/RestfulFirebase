using System;
using System.Collections.Generic;
using System.Text;
using RestfulFirebase.CloudFirestore.Models;

namespace RestfulFirebase.CloudFirestore.Models.Fields
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
