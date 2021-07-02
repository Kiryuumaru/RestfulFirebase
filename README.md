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

* All firebase observable events are executed on the thread that is used to create the object instance. To use in UI safe updates, create the firebase object instances at the UI thread or manually configure the ISyncObject.SyncOperation to use UI thread.
* If ILocalDatabase is provided and implemented with a specific device database persistency implementation, app authentication and the database will be persisted even if the app closes and reopens.

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
                LocalDatabase = "<Your implementation of RestfulFirebase.Local.ILocalDatabase>" // For optional offline persistency 
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

### Realtime

The RealtimeWire holds a database subscription for real-time online and local data updates; Also manages offline persistency and caching for the specified reference node. Persistent data from the app launch will continue to synced if its real-time wire node is created and started.

#### Subscribe

Predefined model values will be overwritten by the database values.

```csharp
using System.Threading.Tasks;
using RestfulFirebase;
using RestfulFirebase.Database.Realtime;

namespace YourNamespace
{
    public static class Program
    {
        public void Subscribe()
        {
            // Creates new realtime wire for https://some-database.firebaseio.com/users/some-uid/pets/dinosaur
            RealtimeWire userWire = app.Database
              .Child("users")
              .Child(app.Auth.Session.LocalId) // User UID
              .Child("pets")
              .Child("dinosaur")
              .AsRealtimeWire();

            // Starts to subscribe and listen for the node`s local and online updates
            userWire.Start();

            // Subscribes model to the node https://some-database.firebaseio.com/users/some-uid/pets/dinosaur
            Dinosaur dinosaur = new Dinosaur();
            userWire.SubModel(dinosaur);
        }
    }
}
```

#### Write and Subscribe

Database values will be overwritten by the predefined model values.

```csharp
using System.Threading.Tasks;
using RestfulFirebase;
using RestfulFirebase.Database.Realtime;

namespace YourNamespace
{
    public static class Program
    {
        public void WriteAndSubscribe()
        {
            // Creates new realtime wire for https://some-database.firebaseio.com/users/some-uid/pets/dinosaur
            RealtimeWire userWire = app.Database
              .Child("users")
              .Child(app.Auth.Session.LocalId) // User UID
              .Child("pets")
              .Child("dinosaur")
              .AsRealtimeWire();

            // Starts to subscribe and listen for the node`s local and online updates
            userWire.Start();

            // Write and subscribes model to the node https://some-database.firebaseio.com/users/some-uid/pets/dinosaur
            Dinosaur dinosaur = new Dinosaur();
            dinosaur.Name = "Megalosaurus";
            userWire.PutModel(dinosaur);
        }
    }
}
```

#### Listen Instance

Creating a listen instance of the existing realtime wire without resubscribing to the node will save you some bandwidth and usage.

```csharp
using System.Threading.Tasks;
using RestfulFirebase;
using RestfulFirebase.Database.Realtime;

namespace YourNamespace
{
    public static class Program
    {
        public void WriteAndSubscribe()
        {
            // Creates new realtime wire for https://some-database.firebaseio.com/users/some-uid/
            RealtimeWire userWire = app.Database
              .Child("users")
              .Child(app.Auth.Session.LocalId) // User UID
              .AsRealtimeWire();

            // Starts to subscribe and listen for the node`s local and online updates
            userWire.Start();

            // Creates a new listen instance without resubscribing to the node.
            RealtimeInstance userDinosaur = userWire
              .Child("pets")
              .Child("dinosaur");
              
            RealtimeInstance userDinosaur1 = userWire
              .Child("pets")
              .Child("dinosaur1");
              
            RealtimeInstance userDinosaur2 = userWire
              .Child("pets")
              .Child("dinosaur2");
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
            // Subscribe to both online and local updates
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
