using RestfulFirebase.Common.Observables;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models
{
    public class FirebaseObjectPager : FirebaseObject
    {
        #region Properties

        public int ObjectPerPages
        {
            get => GetPersistableProperty<int>("opp", 50);
            set
            {

            }
        }

        public int ObjectCount
        {
            get => GetPersistableProperty<int>("oc", 0);
            set => SetPersistableProperty(value, "oc");
        }

        public int PagesCount
        {
            get => GetPersistableProperty<int>("pc", 0);
            set => SetPersistableProperty(value, "pc");
        }

        #endregion

        #region Initializers

        public FirebaseObjectPager(IAttributed attributed)
            : base(attributed)
        {

        }

        public FirebaseObjectPager(string key)
            : base(key)
        {

        }

        #endregion

        #region Methods


        #endregion
    }
}
