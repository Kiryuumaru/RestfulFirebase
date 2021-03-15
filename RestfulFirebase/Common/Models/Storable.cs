using RestfulFirebase.Common.Conversions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public abstract class Storable : ObservableObject
    {
        #region Properties

        public string Id { get; }

        #endregion

        #region Initializers

        protected Storable()
        {
            Id = Helpers.GenerateUID();
        }

        protected Storable(string id)
        {
            Id = id;
        }

        protected Storable(string id, IEnumerable<CellModel> cellModels) : base(cellModels)
        {
            Id = id;
        }

        #endregion

        #region Methods



        #endregion
    }
}
