using RestfulFirebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Test.Utilities
{
    public static class Config
    {
        public static FirebaseConfig YourConfig()
        {
            return new FirebaseConfig()
            {
                ApiKey = "AIzaSyBZfLYmm5SyxmBk0lzBh0_AcDILjOLUD9o",
                DatabaseURL = "https://restfulplayground-default-rtdb.firebaseio.com/",
                StorageBucket = "restfulplayground.appspot.com",
                //CustomAuthLocalDatabase = new DatastoreBlob(true),
                //LocalEncryption = new EncryptTest(true)
            };
        }
    }
}
