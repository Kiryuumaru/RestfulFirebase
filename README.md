# RestfulFirebase

Firebase REST API wired with MVVM observers

**NuGets**

|Name|Info|
| ------------------- | :------------------: |
|RestfulFirebase|[![NuGet](https://buildstats.info/nuget/RestfulFirebase?includePreReleases=true)](https://www.nuget.org/packages/RestfulFirebase/)|

## Get Started

All firebase observable events are executed on thread that was used to create the object instance.
To use in UI safe updates, create the firebase object instances at the UI thread or manually configure the ISyncObject.SyncOperation to use UI thread.

### App Module Sample
```csharp
using RestfulFirebase;

namespace YourNamespace
{
    private static RestfulFirebaseApp app;
    
    public static void Main(string[] args)
    {
        var config = new FirebaseConfig()
        {
            ApiKey = "<Your API key>",
            DatabaseURL = <Your realtime database URL,
            StorageBucket = "<Your storage bucket>",
            LocalDatabase = <Your implementation of ILocalDatabase for offline persistency> // Optional
        };
        app = new RestfulFirebaseApp(config);
    }
}
```

### Authentication
```csharp
using RestfulFirebase;

namespace YourNamespace
{
    public static async Task Authenticate()
    {
        var result = await app.Auth.SignInWithEmailAndPassword("t@st.com", "123123");
    }
}
```

### Realtime Subscription
```csharp
using System.Threading.Tasks;
using RestfulFirebase;
using RestfulFirebase.Database.Realtime;

namespace YourNamespace
{
    private static RealtimeWire userWire;
    
    public static async Task Subscription()
    {
        userWire = app.Database
          .Child("users")
          .Child(app.Auth.Session.LocalId) // User UID
          .AsRealtimeWire();
        
        // Starts to subscribe and listen of the node`s local and online updates
        userWire.Start();
        
        // Creates new listen instance without resubscribing to the node.
        var userDog = userWire
          .Child("pets")
          .Child("dog");
          
        var userDinosaur = userWire
          .Child("pets")
          .Child("dinosaur");
        
        // Writes and subscribes observable model to the realtime instance.
        userDog.PutModel(dog);
        
        // Subscribes observable model to the realtime instance.
        userDinosaur.SubModel(dinosaur);
    }
}
```

### FirebaseObject Sample
```csharp
using RestfulFirebase.Database.Models;

namespace YourNamespace
{
    public class Dinosaur : FirebaseObject
    {
        public string Name
        {
            get => GetFirebasePropertyWithKey<string>("name");
            set => SetFirebasePropertyWithKey(value, "name");
        }
        
        // Uses its property name for firebase key.
        public int Height
        {
            get => GetFirebaseProperty<int>();
            set => GetFirebaseProperty(value);
        }
    }
}
```

### UI safe
```csharp
using RestfulFirebase.Database.Models;

namespace YourNamespace
{
    public class Program
    {
        private Dinosaur dinosaur;

        public void UIThread()
        {
            dinosaur = new Dinosaur();
        }

        public void BackgroundThread()
        {
            dinosaur.PropertyChanged += (s, e) =>
            {
                // Executed on UI thread
            }
            dinosaur.Name = "Megalosaurus";
        }
    }
}
```

Code & Inspiration from the following:
* [firebase-authentication-dotnet](https://github.com/step-up-labs/firebase-authentication-dotnet) by [@step-up-labs](https://github.com/step-up-labs)
* [firebase-database-dotnet](https://github.com/step-up-labs/firebase-database-dotnet) by [@step-up-labs](https://github.com/step-up-labs)
* [firebase-storage-dotnet](https://github.com/step-up-labs/firebase-storage-dotnet) by [@step-up-labs](https://github.com/step-up-labs)


### Want To Support This Project?
All I have ever asked is to be active by submitting bugs, features, and sending those pull requests down!.
