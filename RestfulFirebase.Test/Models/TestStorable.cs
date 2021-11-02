using RestfulFirebase.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Test.Models
{
    public class TestStorable : FirebaseObject
    {
        #region Properties

        public bool IsOk
        {
            get => GetFirebasePropertyWithKey<bool>("isOk");
            set => SetFirebasePropertyWithKey(value, "isOk");
        }

        public TimeSpan Premium
        {
            get => GetFirebasePropertyWithKey<TimeSpan>("premium");
            set => SetFirebasePropertyWithKey(value, "premium");
        }

        public List<TimeSpan> Premiums
        {
            get => GetFirebasePropertyWithKey("premiums", new List<TimeSpan>());
            set => SetFirebasePropertyWithKey(value, "premiums");
        }

        public TimeSpan[] Premiums1
        {
            get => GetFirebasePropertyWithKey("premiums1", new TimeSpan[0]);
            set => SetFirebasePropertyWithKey(value, "premiums1");
        }

        public decimal Num1
        {
            get => GetFirebasePropertyWithKey<decimal>("num1");
            set => SetFirebasePropertyWithKey(value, "num1");
        }

        public decimal Num2
        {
            get => GetFirebasePropertyWithKey<decimal>("num2");
            set => SetFirebasePropertyWithKey(value, "num2");
        }

        public decimal Num3
        {
            get => GetFirebasePropertyWithKey<decimal>("num3");
            set => SetFirebasePropertyWithKey(value, "num3");
        }

        public string Test1
        {
            get => GetFirebasePropertyWithKey<string>("test1");
            set => SetFirebasePropertyWithKey(value, "test1");
        }

        public string Test2
        {
            get => GetFirebasePropertyWithKey<string>("test2");
            set => SetFirebasePropertyWithKey(value, "test2");
        }

        public string Test3
        {
            get => GetFirebasePropertyWithKey<string>("test3");
            set => SetFirebasePropertyWithKey(value, "test3");
        }

        public string Dummy
        {
            get => GetProperty<string>();
            set => SetProperty(value);
        }

        #endregion

        #region Methods

        public TestStorable()
        {

        }

        #endregion
    }

}
