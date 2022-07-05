using RestfulFirebase.Abstraction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RestfulFirebase.CloudFirestore.Models
{ 
    /// <summary>
    /// A position in a query result set.
    /// </summary>
    public sealed class Cursor
    {


        private readonly RepeatedField<Value> values_ = new RepeatedField<Value>();
    }
}
