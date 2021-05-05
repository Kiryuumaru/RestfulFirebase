using RestfulFirebase.Common;
using RestfulFirebase.Common.Observables;
using RestfulFirebase.Database.Models.Primitive;
using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Database.Models.Derived
{
    public class FirebaseObjectPager : FirebaseObject
    {
        #region Properties

        private const string PagesKey = "pages";
        private const int KeysPerPageCount = 10;

        private int PageCount
        {
            get => GetPersistableProperty<int>(PagesKey, 0);
            set => SetPersistableProperty(value, PagesKey);
        }

        public List<string> Keys
        {
            get
            {
                var count = PageCount;
                var keys = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    var data = GetPersistableProperty<string>(PagesKey + i.ToString());
                    var deserialized = Helpers.DeserializeString(data);
                    if (deserialized == null) continue;
                    keys.AddRange(deserialized);
                }
                return keys;
            }
            set
            {
                if (value == null)
                {
                    var count = PageCount;
                    var keys = new List<string>();
                    SetPersistableProperty(0, PagesKey);
                    for (int i = 0; i < count; i++)
                    {
                        DeleteProperty(PagesKey + i.ToString());
                    }
                }
                else
                {
                    var iterations = (value.Count + (KeysPerPageCount - 1)) / KeysPerPageCount;
                    var index = 0;
                    var count = PageCount;
                    var keys = new List<string>();
                    for (int i = 0; i < iterations; i++)
                    {
                        var pageKeys = new List<string>();
                        for (int j = 0; j < KeysPerPageCount; j++)
                        {
                            if (value.Count <= index) break;
                            pageKeys.Add(value[index]);
                            index++;
                        }
                        var page = Helpers.SerializeString(pageKeys.ToArray());
                        SetPersistableProperty(page, (PagesKey + i.ToString()));
                    }
                    SetPersistableProperty(iterations, PagesKey);
                }
            }
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
