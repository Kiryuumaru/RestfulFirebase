using RestfulFirebase.Auth;
using RestfulFirebase.Database;
using RestfulFirebase.Database.Query;
using RestfulFirebase.Database.Offline;
using RestfulFirebase.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestfulFirebase.Common;
using RestfulFirebase;

namespace RestTest
{
    public class TestStorable : Storable
    {
        #region Properties

        public DateTime Created
        {
            get => GetProperty<DateTime>("_cr");
            set => SetProperty(value, "_cr", nameof(Created));
        }
        public DateTime Modified
        {
            get => GetProperty<DateTime>("_md");
            set => SetProperty(value, "_md", nameof(Modified));
        }

        #endregion

        #region Initializers

        public TestStorable() : base(Helpers.GenerateUID())
        {
            Created = DateTime.UtcNow;
            Modified = DateTime.UtcNow;
        }

        public TestStorable(string id) : base(id)
        {
            Created = DateTime.UtcNow;
            Modified = DateTime.UtcNow;
        }

        public TestStorable(string id, DateTime created, DateTime modified) : base(id)
        {
            Created = created;
            Modified = modified;
        }

        public TestStorable(string id, IEnumerable<CellModel> cellModels) : base(id, cellModels)
        {

        }

        #endregion

        #region Methods



        #endregion
    }

    public class Program
    {
        public static void Main(string[] args)
        {
            Run().GetAwaiter().GetResult();
        }

        public static async Task Run()
        {
            var app = new RestfulFirebaseApp(new FirebaseConfig()
            {
                ApiKey = "AIzaSyBZfLYmm5SyxmBk0lzBh0_AcDILjOLUD9o",
                DatabaseURL = "https://restfulplayground-default-rtdb.firebaseio.com/",
                StorageBucket = "restfulplayground.appspot.com"
            });

            await app.Auth.SignInWithEmailAndPasswordAsync("t@st.com", "123123");
            await app.Auth.UpdateProfileAsync("disp", "123123");
            await app.Database.Child("sss").Child("sss").PutAsync(new TestStorable("11"));
            app.Database.Child("sss").AsObservable<TestStorable>().Subscribe(i =>
            {

            });
            await app.Database.Child("sss").Child("sss").PutAsync(new TestStorable("11"));

            //var dinos = await firebase
            //    .Child("ss").AsRealtimeDatabase("", "", StreamingOptions.LatestOnly, InitialPullStrategy.MissingOnly, true)
            //    .OnceAsync<object>();

            int x = 0;

            await Task.Delay(100000);

            //foreach (var dino in dinos)
            //{
            //    Console.WriteLine($"{dino.Key} is {dino.Object.Height}m high.");
            //}
        }
    }
}
