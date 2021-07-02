# RestfulFirebase

Firebase REST API wrapper with streaming API wired with MVVM observers.

**NuGets**

|Name|Info|
| ------------------- | :------------------: |
|RestfulFirebase|[![NuGet](https://buildstats.info/nuget/RestfulFirebase?includePreReleases=true)](https://www.nuget.org/packages/RestfulFirebase/)|

## Installation
```csharp
// Install release version
Install-Package RestfulFirebase

// Install pre-release version
Install-Package RestfulFirebase -pre
```

## Supported frameworks
.NET Standard 2.0 - see https://github.com/dotnet/standard/blob/master/docs/versions.md for compatibility matrix

## Get Started

All firebase observable events are executed on thread that was used to create the object instance.
To use in UI safe updates, create the firebase object instances at the UI thread or manually configure the ISyncObject.SyncOperation to use UI thread.

## Usage

### App Module Sample
```csharp
using RestfulFirebase;

namespace YourNamespace
{
    public static class Program
    {
        private staic RestfulFirebaseApp config;
        private staic RestfulFirebaseApp app;
        
        public static void Main(string[] args)
        {
            config = new FirebaseConfig()
            {
                ApiKey = "<Your API key>",
                DatabaseURL = "<Your realtime database URL>", // Ends with firebaseio.com
                StorageBucket = "<Your storage bucket>", // Ends with appspot.com
                LocalDatabase = "<Your implementation of RestfulFirebase.Local.ILocalDatabase>" // For optional offline persistency and database 
            };
            app = new RestfulFirebaseApp(config);
        }
    }
}
```

### Authentication
```csharp
using RestfulFirebase;

namespace YourNamespace
{
    public static class Program
    {
        public static async void Authenticate()
        {
            await app.Auth.SignInWithEmailAndPassword("t@st.com", "123123");
        }
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
    public static class Program
    {
        public void Subscription()
        {
            // Creates new realtime wire for https://some-database.firebaseio.com/users/some-uid/
            RealtimeWire userWire = app.Database
              .Child("users")
              .Child(app.Auth.Session.LocalId) // User UID
              .AsRealtimeWire();

            // Starts to subscribe and listen for the node`s local and online updates
            userWire.Start();

            // Creates a new listen instance without resubscribing to the node to save bandwidth and usage.
            // This node will be https://some-database.firebaseio.com/users/<UID>/pets/dog
            RealtimeInstance userDog = userWire
              .Child("pets")
              .Child("dog");

            // This node will be https://some-database.firebaseio.com/users/some-uid/pets/dinosaur
            RealtimeInstance userDinosaur = userWire
              .Child("pets")
              .Child("dinosaur");

            // Writes and subscribes model to the node https://some-database.firebaseio.com/users/some-uid/pets/dog.
            // Database values will be overwritten by the predifined values
            Dinosaur dinosaur = new Dinosaur();
            dinosaur.Name = "Megalosaurus";
            userDinosaur.PutModel(dinosaur);

            // Subscribes model to the node https://some-database.firebaseio.com/users/some-uid/pets/dinosaur.
            // Predifined values will be overwritten by the database values.
            Dog dog = new Dog();
            userDog.SubModel(dog);
        }
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
            set => SetFirebaseProperty(value);
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
