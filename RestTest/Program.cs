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

        protected TestStorable() : base(Helpers.GenerateUID())
        {
            Created = DateTime.UtcNow;
            Modified = DateTime.UtcNow;
        }

        protected TestStorable(string id) : base(id)
        {
            Created = DateTime.UtcNow;
            Modified = DateTime.UtcNow;
        }

        protected TestStorable(string id, DateTime created, DateTime modified) : base(id)
        {
            Created = created;
            Modified = modified;
        }

        protected TestStorable(string id, IEnumerable<CellModel> cellModels) : base(id, cellModels)
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

            var authProvider = new FirebaseAuthProvider(new FirebaseConfig("AIzaSyBZfLYmm5SyxmBk0lzBh0_AcDILjOLUD9o"));

            var auth = await authProvider.SignInWithEmailAndPasswordAsync("t@st.com", "123123");

            var firebase = new FirebaseClient(
                "https://restfulplayground-default-rtdb.firebaseio.com/",
                new FirebaseOptions
                {
                    AuthTokenAsyncFactory = () => Task.FromResult(auth.FirebaseToken),
                    OfflineDatabaseFactory = (t, s) => new ConcurrentOfflineDatabase(t, s)
                });

            await firebase.Child("ss").PostAsync("{ \"ss\" : \"ss\" }");

            var dinos = await firebase
                .Child("ss").AsRealtimeDatabase("", "", StreamingOptions.LatestOnly, InitialPullStrategy.MissingOnly, true)
                .OnceAsync<object>();

            int x = 0;

            //foreach (var dino in dinos)
            //{
            //    Console.WriteLine($"{dino.Key} is {dino.Object.Height}m high.");
            //}
        }
    }
}
