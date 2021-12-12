using RestfulFirebase;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Test.Utilities;
using RestfulFirebase.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Local;
using System.ComponentModel;
using ObservableHelpers;
using System.Collections.Specialized;
using RestfulFirebase.Serializers;
using RestfulFirebase.Utilities;

namespace DatabaseTest.ModelsTest
{
    public static class Helpers
    {
        public static Task<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>> AuthenticatedTestApp(string testName, string factName)
        {
            return RestfulFirebase.Test.Helpers.AuthenticatedTestApp(nameof(ModelsTest), testName, factName);
        }

        public static Task CleanTest(
            string testName,
            string factName,
            Func<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>, Task> test)
        {
            return RestfulFirebase.Test.Helpers.CleanTest(nameof(ModelsTest), testName, factName, test);
        }

        public static Task CleanTest(
            string testName,
            string factName,
            Action<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>> test)
        {
            return RestfulFirebase.Test.Helpers.CleanTest(nameof(ModelsTest), testName, factName, test);
        }
    }

    public class Dinosaur
    {
        public string? Name { get; set; }

        public int Height { get; set; }

        public override bool Equals(object? obj)
        {
            if (obj is Dinosaur dino)
            {
                if (dino.Name == Name && dino.Height == Height)
                {
                    return true;
                }
            }

            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Height);
        }
    }

    public class DinosaurSerializer : ISerializer<Dinosaur>
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public Dinosaur Deserialize(string data, Dinosaur defaultValue = default)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            if (data == null)
            {
                return defaultValue;
            }

            string[] deserialized = StringUtilities.Deserialize(data);

            if (deserialized.Length == 2)
            {
                if (int.TryParse(deserialized[1], out int height))
                {
                    return new Dinosaur()
                    {
                        Name = deserialized[0],
                        Height = height
                    };
                }
            }

            return defaultValue;
        }

        public string? Serialize(Dinosaur value)
        {
            if (value == null)
            {
                return null;
            }

            return StringUtilities.Serialize(value.Name, value.Height.ToString());
        }
    }

    public class Person : FirebaseObject
    {
        #region Properties

        public string FirstName
        {
            get => GetFirebasePropertyWithKey<string>("first_name");
            set => SetFirebasePropertyWithKey(value, "first_name");
        }

        public string LastName
        {
            get => GetFirebasePropertyWithKey<string>("last_name");
            set => SetFirebasePropertyWithKey(value, "last_name");
        }

        public DateTime Birthdate
        {
            get => GetFirebasePropertyWithKey<DateTime>("birthdate");
            set => SetFirebasePropertyWithKey(value, "birthdate");
        }

        public override bool Equals(object? obj)
        {
            return obj is Person person &&
                   FirstName == person.FirstName &&
                   LastName == person.LastName &&
                   Birthdate == person.Birthdate;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(FirstName, LastName, Birthdate);
        }

        #endregion
    }

    public class Couple : Person
    {
        #region Properties

        public Person Partner
        {
            get => GetFirebasePropertyWithKey<Person>("partner");
            set => SetFirebasePropertyWithKey(value, "partner");
        }

        #endregion
    }

    public class PersonWithPet : Person
    {
        #region Properties

        public Dinosaur Pet
        {
            get => GetFirebasePropertyWithKey<Dinosaur>("pet");
            set => SetFirebasePropertyWithKey(value, "pet");
        }

        public override bool Equals(object? obj)
        {
            return obj is PersonWithPet pet &&
                   base.Equals(obj) &&
                   EqualityComparer<Dinosaur>.Default.Equals(Pet, pet.Pet);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), Pet);
        }

        #endregion
    }

    public class Group : FirebaseObject
    {
        public string GroupName
        {
            get => GetFirebaseProperty<string>();
            set => SetFirebaseProperty(value);
        }

        public FirebaseDictionary<Person> Members
        {
            get => GetFirebaseProperty<FirebaseDictionary<Person>>();
            set => SetFirebaseProperty(value);
        }
    }

    public class FirebasePropertyTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(FirebasePropertyTest), nameof(Normal), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var model1 = new FirebaseProperty<string>();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                model1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };

                appInstance1.wire.SubModel(model1);

                model1.Value = "test1";
                model1.Value = "test1";

                Assert.Equal("test1", model1.Value);

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("test1", model1.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void PutOverwrite()
        {
            await Helpers.CleanTest(nameof(FirebasePropertyTest), nameof(PutOverwrite), async generator =>
            {
                var appInstance1 = await generator(null);
                var appInstance2 = await generator(null);

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;
                appInstance2.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                appInstance1.wire.Start();
                appInstance2.wire.Start();

                var model1 = new FirebaseProperty<string>();
                var model2 = new FirebaseProperty<string>();

                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                List<PropertyChangedEventArgs> propertyChanges2 = new List<PropertyChangedEventArgs>();

                model1.PropertyChanged += (s, e) => propertyChanges1.Add(e);
                model2.PropertyChanged += (s, e) => propertyChanges2.Add(e);

                appInstance1.wire.SubModel(model1);

                model1.Value = "test1";
                model2.Value = "test2";

                await Task.Delay(5000);

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 10;
                Assert.True(await appInstance1.wire.WaitForSynced(TimeSpan.FromMinutes(1)));
                Assert.True(await appInstance2.wire.WaitForSynced(TimeSpan.FromMinutes(1)));

                appInstance2.wire.PutModel(model2);

                appInstance2.app.Config.DatabaseMaxConcurrentSyncWrites = 10;
                Assert.True(await appInstance1.wire.WaitForSynced(TimeSpan.FromMinutes(1)));

                await Task.Delay(5000);

                Assert.Equal("test2", model1.Value);
                Assert.Equal("test2", model2.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });
                Assert.Empty(propertyChanges2);

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                Assert.Collection(appInstance2.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void Cascade()
        {
            await Helpers.CleanTest(nameof(FirebasePropertyTest), nameof(Cascade), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var model1 = new FirebaseProperty<FirebaseProperty<string>>();
                var subModel1 = new FirebaseProperty<string>();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                model1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };
                List<PropertyChangedEventArgs> subPropertyChanges1 = new List<PropertyChangedEventArgs>();
                subModel1.PropertyChanged += (s, e) =>
                {
                    subPropertyChanges1.Add(e);
                };

                appInstance1.wire.SubModel(model1);

                subModel1.Value = "test1";
                subModel1.Value = "test1";

                model1.Value = subModel1;

                Assert.Equal("test1", model1.Value.Value);

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("test1", model1.Value.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });
                Assert.Collection(subPropertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(subModel1.Value), i.PropertyName);
                    });

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void Nullable()
        {
            await Helpers.CleanTest(nameof(FirebasePropertyTest), nameof(Nullable), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var model1 = new FirebaseProperty<string>();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                model1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };

                appInstance1.wire.SubModel(model1);

                model1.Value = "test1";

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                await Task.Delay(5000);

                appInstance1.dataChanges.Clear();
                propertyChanges1.Clear();

                Assert.False(model1.IsNull());
                Assert.True(model1.SetNull());
                Assert.False(model1.SetNull());
                Assert.True(model1.IsNull());

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void CrossSync()
        {
            await Helpers.CleanTest(nameof(FirebasePropertyTest), nameof(CrossSync), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                var model1 = new FirebaseProperty<string>();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                model1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };
                var model2 = new FirebaseProperty<string>();
                List<PropertyChangedEventArgs> propertyChanges2 = new List<PropertyChangedEventArgs>();
                model2.PropertyChanged += (s, e) =>
                {
                    propertyChanges2.Add(e);
                };

                appInstance1.wire.SubModel(model1);
                appInstance2.wire.SubModel(model2);

                model1.Value = "test1";
                model1.Value = "test1";

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                Assert.Equal("test1", model2.Value);
                Assert.Equal(model1.Value, model2.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });
                Assert.Collection(propertyChanges2,
                    i =>
                    {
                        Assert.Equal(nameof(model2.Value), i.PropertyName);
                    });
                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });

                Assert.Collection(appInstance2.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                model2.Value = "test2";
                model2.Value = "test2";

                Assert.True(await model2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                Assert.Equal("test2", model1.Value);
                Assert.Equal(model1.Value, model2.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });
                Assert.Collection(propertyChanges2,
                    i =>
                    {
                        Assert.Equal(nameof(model2.Value), i.PropertyName);
                    });
                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                Assert.Collection(appInstance2.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void CustomSerializable()
        {
            await Helpers.CleanTest(nameof(FirebasePropertyTest), nameof(CustomSerializable), async generator =>
            {
                Serializer.Register(new DinosaurSerializer());

                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var model1 = new FirebaseProperty<Dinosaur>();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                model1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };

                appInstance1.wire.SubModel(model1);

                Dinosaur dino1 = new Dinosaur()
                {
                    Name = "Megalosaurus",
                    Height = 100
                };

                model1.Value = dino1;
                model1.Value = dino1;

                Assert.Equal(dino1, model1.Value);

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal(dino1, model1.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void ConflictCreateTest()
        {
            await Helpers.CleanTest(nameof(FirebasePropertyTest), nameof(ConflictCreateTest), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                var model1 = new FirebaseProperty<string>();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                model1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };
                var model2 = new FirebaseProperty<string>();
                List<PropertyChangedEventArgs> propertyChanges2 = new List<PropertyChangedEventArgs>();
                model2.PropertyChanged += (s, e) =>
                {
                    propertyChanges2.Add(e);
                };

                appInstance1.wire.SubModel(model1);
                appInstance2.wire.SubModel(model2);

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                Assert.True(await model2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

                model2.Value = "test2";
                model2.Value = "test2";

                Assert.NotEqual(model1.Value, model2.Value);

                appInstance2.wire.Start();

                Assert.True(await model2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                Assert.Equal("test2", model1.Value);
                Assert.Equal(model1.Value, model2.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });
                Assert.Collection(propertyChanges2,
                    i =>
                    {
                        Assert.Equal(nameof(model2.Value), i.PropertyName);
                    });
                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                Assert.Collection(appInstance2.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });

                appInstance1.wire.SetNull();
                appInstance2.wire.SetNull();
                Assert.True(await appInstance1.wire.WaitForSynced(TimeSpan.FromMinutes(1)));
                Assert.True(await appInstance2.wire.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);
                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

                model1.Value = "test3";
                model1.Value = "test3";

                model2.Value = "test3Conflict";
                model2.Value = "test3Conflict";

                Assert.NotEqual(model1.Value, model2.Value);

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                appInstance2.wire.Start();

                Assert.True(await model2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("test3", model2.Value);
                Assert.Equal(model1.Value, model2.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });
                Assert.Collection(propertyChanges2,
                    i =>
                    {
                        Assert.Equal(nameof(model2.Value), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(model2.Value), i.PropertyName);
                    });
                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                Assert.Collection(appInstance2.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void ConflictUpdateTest()
        {
            await Helpers.CleanTest(nameof(FirebasePropertyTest), nameof(ConflictUpdateTest), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                var model1 = new FirebaseProperty<string>();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                model1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };
                var model2 = new FirebaseProperty<string>();
                List<PropertyChangedEventArgs> propertyChanges2 = new List<PropertyChangedEventArgs>();
                model2.PropertyChanged += (s, e) =>
                {
                    propertyChanges2.Add(e);
                };

                appInstance1.wire.SubModel(model1);
                appInstance2.wire.SubModel(model2);

                model1.Value = "test1";
                model2.Value = "test1";

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                Assert.True(await model2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

                model2.Value = "test2";
                model2.Value = "test2";

                Assert.NotEqual(model1.Value, model2.Value);

                appInstance2.wire.Start();

                Assert.True(await model2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                Assert.Equal("test2", model1.Value);
                Assert.Equal(model1.Value, model2.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });
                Assert.Collection(propertyChanges2,
                    i =>
                    {
                        Assert.Equal(nameof(model2.Value), i.PropertyName);
                    });
                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                Assert.Collection(appInstance2.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

                model1.Value = "test3";
                model1.Value = "test3";

                model2.Value = "test3Conflict";
                model2.Value = "test3Conflict";

                Assert.NotEqual(model1.Value, model2.Value);

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                appInstance2.wire.Start();

                Assert.True(await model2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("test3", model2.Value);
                Assert.Equal(model1.Value, model2.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });
                Assert.Collection(propertyChanges2,
                    i =>
                    {
                        Assert.Equal(nameof(model2.Value), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(model2.Value), i.PropertyName);
                    });
                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                Assert.Collection(appInstance2.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void ConflictDeleteTest()
        {
            await Helpers.CleanTest(nameof(FirebasePropertyTest), nameof(ConflictDeleteTest), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                var model1 = new FirebaseProperty<string>();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                model1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };
                var model2 = new FirebaseProperty<string>();
                List<PropertyChangedEventArgs> propertyChanges2 = new List<PropertyChangedEventArgs>();
                model2.PropertyChanged += (s, e) =>
                {
                    propertyChanges2.Add(e);
                };

                appInstance1.wire.SubModel(model1);
                appInstance2.wire.SubModel(model2);

                model1.Value = "test1";
                model2.Value = "test1";

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                Assert.True(await model2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

                model1.Value = "test2";
                model1.Value = "test2";

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                model2.Value = null;
                model2.Value = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                Assert.NotEqual(model1.Value, model2.Value);

                Assert.True(await model1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                appInstance2.wire.Start();

                Assert.True(await model2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("test2", model2.Value);
                Assert.Equal(model1.Value, model2.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });
                Assert.Collection(propertyChanges2,
                    i =>
                    {
                        Assert.Equal(nameof(model2.Value), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(model2.Value), i.PropertyName);
                    });
                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                Assert.Collection(appInstance2.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                model2.Value = null;
                model2.Value = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                Assert.NotEqual(model1.Value, model2.Value);

                appInstance2.wire.Start();

                Assert.True(await model2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                Assert.Null(model1.Value);
                Assert.Equal(model1.Value, model2.Value);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(model1.Value), i.PropertyName);
                    });
                Assert.Collection(propertyChanges2,
                    i =>
                    {
                        Assert.Equal(nameof(model2.Value), i.PropertyName);
                    });
                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                Assert.Collection(appInstance2.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    });
                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
            });
        }
    }

    public class FirebaseObjectTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(FirebaseObjectTest), nameof(Normal), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;
                appInstance1.wire.Start();

                var person1 = new Person();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                person1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };

                appInstance1.wire.SubModel(person1);

                DateTime date = DateTime.Now;
                person1.FirstName = "John";
                person1.LastName = "Doe";
                person1.Birthdate = date;

                await Task.Delay(5000);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(person1.FirstName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person1.LastName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person1.Birthdate), i.PropertyName);
                    });
                propertyChanges1.Clear();

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("first_name", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("last_name", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("birthdate", i.Path[0]);
                    });
                appInstance1.dataChanges.Clear();

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 10;
                Assert.True(await appInstance1.wire.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("John", person1.FirstName);
                Assert.Equal("Doe", person1.LastName);
                Assert.Equal(date, person1.Birthdate);

                Assert.Empty(propertyChanges1);

                Assert.Equal(3, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void PutOverwrite()
        {
            await Helpers.CleanTest(nameof(FirebaseObjectTest), nameof(PutOverwrite), async generator =>
            {
                var appInstance1 = await generator(null);
                var appInstance2 = await generator(null);

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;
                appInstance2.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                appInstance1.wire.Start();
                appInstance2.wire.Start();

                var person1 = new Person();
                var person2 = new Person();

                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                List<PropertyChangedEventArgs> propertyChanges2 = new List<PropertyChangedEventArgs>();

                person1.PropertyChanged += (s, e) => propertyChanges1.Add(e);
                person2.PropertyChanged += (s, e) => propertyChanges2.Add(e);

                appInstance1.wire.SubModel(person1);

                DateTime date1 = DateTime.Now;
                DateTime date2 = DateTime.Now;

                person1.FirstName = "John";
                person1.LastName = "Doe";
                person1.Birthdate = date1;

                person2.FirstName = "Peter";
                person2.LastName = "Parker";
                person2.Birthdate = date2;

                await Task.Delay(5000);

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 10;
                Assert.True(await appInstance1.wire.WaitForSynced(TimeSpan.FromMinutes(1)));
                Assert.True(await appInstance2.wire.WaitForSynced(TimeSpan.FromMinutes(1)));

                appInstance2.wire.PutModel(person2);

                appInstance2.app.Config.DatabaseMaxConcurrentSyncWrites = 10;
                Assert.True(await appInstance2.wire.WaitForSynced(TimeSpan.FromMinutes(1)));

                await Task.Delay(5000);

                Assert.Equal("Peter", person1.FirstName);
                Assert.Equal("Parker", person1.LastName);
                Assert.Equal(date2, person1.Birthdate);

                Assert.Equal("Peter", person2.FirstName);
                Assert.Equal("Parker", person2.LastName);
                Assert.Equal(date2, person2.Birthdate);

                Assert.Equal(3, propertyChanges1.Count);
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.FirstName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.LastName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.Birthdate));

                Assert.Empty(propertyChanges2);

                Assert.Equal(6, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                Assert.Equal(12, appInstance2.dataChanges.Count);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void Cascade()
        {
            await Helpers.CleanTest(nameof(FirebaseObjectTest), nameof(Cascade), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;
                appInstance1.wire.Start();

                var couple1 = new Couple();
                var person1 = new Person();
                List<PropertyChangedEventArgs> couplePropertyChanges1 = new List<PropertyChangedEventArgs>();
                couple1.PropertyChanged += (s, e) =>
                {
                    couplePropertyChanges1.Add(e);
                };
                List<PropertyChangedEventArgs> personPropertyChanges1 = new List<PropertyChangedEventArgs>();
                person1.PropertyChanged += (s, e) =>
                {
                    personPropertyChanges1.Add(e);
                };

                appInstance1.wire.SubModel(couple1);

                DateTime date = DateTime.Now;
                person1.FirstName = "John";
                person1.LastName = "Doe";
                person1.Birthdate = date;

                couple1.FirstName = "Lara";
                couple1.LastName = "Croft";
                couple1.Birthdate = date;

                couple1.Partner = person1;

                await Task.Delay(5000);

                Assert.Collection(couplePropertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(couple1.FirstName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(couple1.LastName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(couple1.Birthdate), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(couple1.Partner), i.PropertyName);
                    });
                couplePropertyChanges1.Clear();
                Assert.Collection(personPropertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(person1.FirstName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person1.LastName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person1.Birthdate), i.PropertyName);
                    });
                personPropertyChanges1.Clear();

                Assert.Equal(13, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "partner");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "partner" && i.Path[1] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "partner" && i.Path[1] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "partner" && i.Path[1] == "birthdate");
                appInstance1.dataChanges.Clear();

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 10;
                Assert.True(await appInstance1.wire.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("John", person1.FirstName);
                Assert.Equal("Doe", person1.LastName);
                Assert.Equal(date, person1.Birthdate);

                Assert.Equal("Lara", couple1.FirstName);
                Assert.Equal("Croft", couple1.LastName);
                Assert.Equal(date, couple1.Birthdate);
                Assert.Equal("John", couple1.Partner.FirstName);
                Assert.Equal("Doe", couple1.Partner.LastName);
                Assert.Equal(date, couple1.Partner.Birthdate);

                Assert.Empty(couplePropertyChanges1);
                Assert.Empty(personPropertyChanges1);

                Assert.Equal(6, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "partner" && i.Path[1] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "partner" && i.Path[1] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "partner" && i.Path[1] == "birthdate");
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void Nullable()
        {
            await Helpers.CleanTest(nameof(FirebaseObjectTest), nameof(Nullable), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var person1 = new Person();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                person1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };

                appInstance1.wire.SubModel(person1);

                DateTime date = DateTime.Now;
                person1.FirstName = "John";
                person1.LastName = "Doe";
                person1.Birthdate = date;

                Assert.True(await person1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                await Task.Delay(5000);

                appInstance1.dataChanges.Clear();
                propertyChanges1.Clear();

                Assert.False(person1.IsNull());
                Assert.True(person1.SetNull());
                Assert.False(person1.SetNull());
                Assert.True(person1.IsNull());

                Assert.True(await person1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(person1.FirstName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person1.LastName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person1.Birthdate), i.PropertyName);
                    });
                propertyChanges1.Clear();

                Assert.Equal(9, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void CrossSync()
        {
            await Helpers.CleanTest(nameof(FirebaseObjectTest), nameof(CrossSync), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                var person1 = new Person();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                person1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };
                var person2 = new Person();
                List<PropertyChangedEventArgs> propertyChanges2 = new List<PropertyChangedEventArgs>();
                person2.PropertyChanged += (s, e) =>
                {
                    propertyChanges2.Add(e);
                };

                appInstance1.wire.SubModel(person1);
                appInstance2.wire.SubModel(person2);

                DateTime date1 = DateTime.Now;

                person1.FirstName = "John";
                person1.LastName = "Doe";
                person1.Birthdate = date1;

                Assert.True(await person1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                Assert.Equal("John", person2.FirstName);
                Assert.Equal("Doe", person2.LastName);
                Assert.Equal(date1, person2.Birthdate);

                Assert.Equal(person1.FirstName, person2.FirstName);
                Assert.Equal(person1.LastName, person2.LastName);
                Assert.Equal(person1.Birthdate, person2.Birthdate);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(person1.FirstName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person1.LastName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person1.Birthdate), i.PropertyName);
                    });

                Assert.Equal(3, propertyChanges2.Count);
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.FirstName));
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.LastName));
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.Birthdate));

                Assert.Equal(9, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                Assert.Equal(6, appInstance2.dataChanges.Count);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                DateTime date2 = DateTime.Now;

                person2.FirstName = "Peter";
                person2.LastName = "Parker";
                person2.Birthdate = date2;

                Assert.True(await person2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                Assert.Equal("Peter", person1.FirstName);
                Assert.Equal("Parker", person1.LastName);
                Assert.Equal(date2, person1.Birthdate);

                Assert.Equal(person1.FirstName, person2.FirstName);
                Assert.Equal(person1.LastName, person2.LastName);
                Assert.Equal(person1.Birthdate, person2.Birthdate);

                Assert.Collection(propertyChanges2,
                    i =>
                    {
                        Assert.Equal(nameof(person2.FirstName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person2.LastName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person2.Birthdate), i.PropertyName);
                    });

                Assert.Equal(3, propertyChanges1.Count);
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person2.FirstName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person2.LastName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person2.Birthdate));

                Assert.Equal(3, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                Assert.Equal(6, appInstance2.dataChanges.Count);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void CustomSerializable()
        {
            await Helpers.CleanTest(nameof(FirebaseObjectTest), nameof(CustomSerializable), async generator =>
            {
                Serializer.Register(new DinosaurSerializer());

                var appInstance1 = await generator(null);
                appInstance1.wire.Start();
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                var person1 = new PersonWithPet();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                person1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };

                appInstance1.wire.SubModel(person1);

                DateTime date1 = DateTime.Now;
                Dinosaur dino1 = new Dinosaur()
                {
                    Name = "Megalosaurus",
                    Height = 100
                };

                person1.FirstName = "Peter";
                person1.LastName = "Parker";
                person1.Birthdate = date1;
                person1.Pet = dino1;

                await Task.Delay(5000);

                Assert.Equal("Peter", person1.FirstName);
                Assert.Equal("Parker", person1.LastName);
                Assert.Equal(date1, person1.Birthdate);
                Assert.Equal(dino1, person1.Pet);

                Assert.Equal("Peter", person1.FirstName);
                Assert.Equal("Parker", person1.LastName);
                Assert.Equal(date1, person1.Birthdate);
                Assert.Equal(dino1, person1.Pet);

                Assert.Collection(propertyChanges1,
                    i =>
                    {
                        Assert.Equal(nameof(person1.FirstName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person1.LastName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person1.Birthdate), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person1.Pet), i.PropertyName);
                    });
                propertyChanges1.Clear();

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("first_name", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("last_name", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("birthdate", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("pet", i.Path[0]);
                    });
                appInstance1.dataChanges.Clear();

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 10;
                Assert.True(await appInstance1.wire.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("Peter", person1.FirstName);
                Assert.Equal("Parker", person1.LastName);
                Assert.Equal(date1, person1.Birthdate);
                Assert.Equal(dino1, person1.Pet);

                Assert.Empty(propertyChanges1);

                Assert.Equal(4, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "pet");
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void ConflictCreateTest()
        {
            await Helpers.CleanTest(nameof(FirebaseObjectTest), nameof(ConflictCreateTest), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                var person1 = new Person();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                person1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };
                var person2 = new Person();
                List<PropertyChangedEventArgs> propertyChanges2 = new List<PropertyChangedEventArgs>();
                person2.PropertyChanged += (s, e) =>
                {
                    propertyChanges2.Add(e);
                };

                appInstance1.wire.SubModel(person1);
                appInstance2.wire.SubModel(person2);

                Assert.True(await person1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                Assert.True(await person2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

                DateTime date2 = DateTime.Now;

                person2.FirstName = "John";
                person2.LastName = "Doe";
                person2.Birthdate = date2;

                Assert.NotEqual(person1.FirstName, person2.FirstName);
                Assert.NotEqual(person1.LastName, person2.LastName);
                Assert.NotEqual(person1.Birthdate, person2.Birthdate);

                appInstance2.wire.Start();

                Assert.True(await person2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                Assert.Equal("John", person1.FirstName);
                Assert.Equal("Doe", person1.LastName);
                Assert.Equal(date2, person1.Birthdate);

                Assert.Equal(person1.FirstName, person2.FirstName);
                Assert.Equal(person1.LastName, person2.LastName);
                Assert.Equal(person1.Birthdate, person2.Birthdate);

                Assert.Collection(propertyChanges2,
                    i =>
                    {
                        Assert.Equal(nameof(person2.FirstName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person2.LastName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person2.Birthdate), i.PropertyName);
                    });

                Assert.Equal(3, propertyChanges1.Count);
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.FirstName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.LastName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.Birthdate));

                Assert.Equal(6, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                Assert.Equal(9, appInstance2.dataChanges.Count);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                appInstance1.wire.SetNull();
                appInstance2.wire.SetNull();
                Assert.True(await appInstance1.wire.WaitForSynced(TimeSpan.FromMinutes(1)));
                Assert.True(await appInstance2.wire.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

                DateTime date1 = DateTime.Now;

                person1.FirstName = "Peter";
                person1.LastName = "Parker";
                person1.Birthdate = date1;

                person2.FirstName = "John";
                person2.LastName = "Doe";
                person2.Birthdate = date2;

                Assert.NotEqual(person1.FirstName, person2.FirstName);
                Assert.NotEqual(person1.LastName, person2.LastName);
                Assert.NotEqual(person1.Birthdate, person2.Birthdate);

                Assert.True(await person1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                appInstance2.wire.Start();

                Assert.True(await person2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("Peter", person1.FirstName);
                Assert.Equal("Parker", person1.LastName);
                Assert.Equal(date1, person1.Birthdate);

                Assert.Equal(person1.FirstName, person2.FirstName);
                Assert.Equal(person1.LastName, person2.LastName);
                Assert.Equal(person1.Birthdate, person2.Birthdate);

                Assert.Equal(3, propertyChanges1.Count);
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.FirstName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.LastName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.Birthdate));

                Assert.Equal(6, propertyChanges2.Count);
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.FirstName));
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.LastName));
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.Birthdate));

                Assert.Equal(9, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                Assert.Equal(9, appInstance2.dataChanges.Count);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void ConflictUpdateTest()
        {
            await Helpers.CleanTest(nameof(FirebaseObjectTest), nameof(ConflictUpdateTest), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                var person1 = new Person();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                person1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };
                var person2 = new Person();
                List<PropertyChangedEventArgs> propertyChanges2 = new List<PropertyChangedEventArgs>();
                person2.PropertyChanged += (s, e) =>
                {
                    propertyChanges2.Add(e);
                };

                appInstance1.wire.SubModel(person1);
                appInstance2.wire.SubModel(person2);

                DateTime date1 = DateTime.Now;

                person1.FirstName = "John";
                person1.LastName = "Doe";
                person1.Birthdate = date1;

                Assert.True(await person1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                Assert.True(await person2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

                DateTime date2 = DateTime.Now;

                person2.FirstName = "Peter";
                person2.LastName = "Parker";
                person2.Birthdate = date2;

                Assert.NotEqual(person1.FirstName, person2.FirstName);
                Assert.NotEqual(person1.LastName, person2.LastName);
                Assert.NotEqual(person1.Birthdate, person2.Birthdate);

                appInstance2.wire.Start();

                Assert.True(await person2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                Assert.Equal("Peter", person1.FirstName);
                Assert.Equal("Parker", person1.LastName);
                Assert.Equal(date2, person1.Birthdate);

                Assert.Equal(person1.FirstName, person2.FirstName);
                Assert.Equal(person1.LastName, person2.LastName);
                Assert.Equal(person1.Birthdate, person2.Birthdate);

                Assert.Collection(propertyChanges2,
                    i =>
                    {
                        Assert.Equal(nameof(person2.FirstName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person2.LastName), i.PropertyName);
                    },
                    i =>
                    {
                        Assert.Equal(nameof(person2.Birthdate), i.PropertyName);
                    });

                Assert.Equal(3, propertyChanges1.Count);
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.FirstName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.LastName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.Birthdate));

                Assert.Equal(3, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                Assert.Equal(6, appInstance2.dataChanges.Count);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

                DateTime dateConflict1 = DateTime.Now;
                DateTime dateConflict2 = DateTime.UtcNow;

                person1.FirstName = "Doc";
                person1.LastName = "Octo";
                person1.Birthdate = dateConflict1;

                person2.FirstName = "Electro";
                person2.LastName = "Man";
                person2.Birthdate = dateConflict2;

                Assert.NotEqual(person1.FirstName, person2.FirstName);
                Assert.NotEqual(person1.LastName, person2.LastName);
                Assert.NotEqual(person1.Birthdate, person2.Birthdate);

                Assert.True(await person1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                appInstance2.wire.Start();

                Assert.True(await person2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("Doc", person2.FirstName);
                Assert.Equal("Octo", person2.LastName);
                Assert.Equal(dateConflict1, person2.Birthdate);

                Assert.Equal(person1.FirstName, person2.FirstName);
                Assert.Equal(person1.LastName, person2.LastName);
                Assert.Equal(person1.Birthdate, person2.Birthdate);

                Assert.Equal(3, propertyChanges1.Count);
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.FirstName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.LastName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.Birthdate));

                Assert.Equal(6, propertyChanges2.Count);
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.FirstName));
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.LastName));
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.Birthdate));

                Assert.Equal(6, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                Assert.Equal(6, appInstance2.dataChanges.Count);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void ConflictDeleteTest()
        {
            await Helpers.CleanTest(nameof(FirebaseObjectTest), nameof(ConflictDeleteTest), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.wire.Start();

                var appInstance2 = await generator(null);
                appInstance2.wire.Start();

                var person1 = new Person();
                List<PropertyChangedEventArgs> propertyChanges1 = new List<PropertyChangedEventArgs>();
                person1.PropertyChanged += (s, e) =>
                {
                    propertyChanges1.Add(e);
                };
                var person2 = new Person();
                List<PropertyChangedEventArgs> propertyChanges2 = new List<PropertyChangedEventArgs>();
                person2.PropertyChanged += (s, e) =>
                {
                    propertyChanges2.Add(e);
                };

                appInstance1.wire.SubModel(person1);
                appInstance2.wire.SubModel(person2);

                DateTime date1 = DateTime.Now;

                person1.FirstName = "John";
                person1.LastName = "Doe";
                person1.Birthdate = date1;

                Assert.True(await person1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                Assert.True(await person2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

                DateTime date2 = DateTime.Now;

                person1.FirstName = "Peter";
                person1.LastName = "Parker";
                person1.Birthdate = date2;

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                person2.FirstName = default;
                person2.LastName = default;
                person2.Birthdate = default;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                Assert.NotEqual(person1.FirstName, person2.FirstName);
                Assert.NotEqual(person1.LastName, person2.LastName);
                Assert.NotEqual(person1.Birthdate, person2.Birthdate);

                Assert.True(await person1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                appInstance2.wire.Start();

                Assert.True(await person2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("Peter", person2.FirstName);
                Assert.Equal("Parker", person2.LastName);
                Assert.Equal(date2, person2.Birthdate);

                Assert.Equal(person1.FirstName, person2.FirstName);
                Assert.Equal(person1.LastName, person2.LastName);
                Assert.Equal(person1.Birthdate, person2.Birthdate);

                Assert.Equal(3, propertyChanges1.Count);
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.FirstName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.LastName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.Birthdate));

                Assert.Equal(6, propertyChanges2.Count);
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.FirstName));
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.LastName));
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.Birthdate));

                Assert.Equal(6, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                Assert.Equal(6, appInstance2.dataChanges.Count);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance2.wire.Stop();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
                person1.FirstName = default;
                person1.LastName = default;
                person1.Birthdate = default;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

                Assert.NotEqual(person1.FirstName, person2.FirstName);
                Assert.NotEqual(person1.LastName, person2.LastName);
                Assert.NotEqual(person1.Birthdate, person2.Birthdate);

                appInstance2.wire.Start();

                Assert.True(await person1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));
                await Task.Delay(5000);

                Assert.Null(person2.FirstName);
                Assert.Null(person2.LastName);
                Assert.Equal(default, person2.Birthdate);

                Assert.Equal(person1.FirstName, person2.FirstName);
                Assert.Equal(person1.LastName, person2.LastName);
                Assert.Equal(person1.Birthdate, person2.Birthdate);

                Assert.Equal(3, propertyChanges1.Count);
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.FirstName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.LastName));
                Assert.Contains(propertyChanges1, i => i.PropertyName == nameof(person1.Birthdate));

                Assert.Equal(3, propertyChanges2.Count);
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.FirstName));
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.LastName));
                Assert.Contains(propertyChanges2, i => i.PropertyName == nameof(person2.Birthdate));

                Assert.Equal(8, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                Assert.Equal(5, appInstance2.dataChanges.Count);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "first_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "last_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "birthdate");

                propertyChanges1.Clear();
                propertyChanges2.Clear();
                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
            });
        }
    }

    public class FirebaseDictionaryTest
    {
        [Fact]
        public async void Normal()
        {
            await Helpers.CleanTest(nameof(FirebaseDictionaryTest), nameof(Normal), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;
                appInstance1.wire.Start();

                FirebaseDictionary<string> dictionary = new FirebaseDictionary<string>();
                List<NotifyCollectionChangedEventArgs> collectionChanges1 = new List<NotifyCollectionChangedEventArgs>();
                dictionary.CollectionChanged += (s, e) => collectionChanges1.Add(e);

                appInstance1.wire.SubModel(dictionary);

                dictionary.Add("key1", "test1");
                dictionary.Add("key2", "test2");

                await Task.Delay(5000);

                Assert.Equal(2, dictionary.Count);
                Assert.Contains(dictionary, i => i.Key == "key1" && i.Value == "test1");
                Assert.Contains(dictionary, i => i.Key == "key2" && i.Value == "test2");

                Assert.Collection(collectionChanges1,
                    i =>
                    {
                        Assert.Equal(NotifyCollectionChangedAction.Add, i.Action);
                        Assert.Equal(0, i.NewStartingIndex);
                        Assert.Equal(1, i.NewItems?.Count);
                        Assert.Equal(KeyValuePair.Create("key1", "test1"), i.NewItems?[0]);
                        Assert.Equal(-1, i.OldStartingIndex);
                        Assert.Equal(null, i.OldItems?.Count);
                    },
                    i =>
                    {
                        Assert.Equal(NotifyCollectionChangedAction.Add, i.Action);
                        Assert.Equal(1, i.NewStartingIndex);
                        Assert.Equal(1, i.NewItems?.Count);
                        Assert.Equal(KeyValuePair.Create("key2", "test2"), i.NewItems?[0]);
                        Assert.Equal(-1, i.OldStartingIndex);
                        Assert.Equal(null, i.OldItems?.Count);
                    });
                collectionChanges1.Clear();

                Assert.Collection(appInstance1.dataChanges,
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("key1", i.Path[0]);
                    },
                    i =>
                    {
                        Assert.Empty(i.Path);
                    },
                    i =>
                    {
                        Assert.Equal(1, i.Path.Length);
                        Assert.Equal("key2", i.Path[0]);
                    });
                appInstance1.dataChanges.Clear();

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 10;
                Assert.True(await appInstance1.wire.WaitForSynced(TimeSpan.FromMinutes(1)));
            });
        }

        [Fact]
        public async void PutOverwrite()
        {
            await Helpers.CleanTest(nameof(FirebaseDictionaryTest), nameof(PutOverwrite), async generator =>
            {
                var appInstance1 = await generator(null);
                var appInstance2 = await generator(null);

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;
                appInstance2.app.Config.DatabaseMaxConcurrentSyncWrites = 0;

                appInstance1.wire.Start();
                appInstance2.wire.Start();

                var dictionary1 = new FirebaseDictionary<string>();
                var dictionary2 = new FirebaseDictionary<string>();

                List<NotifyCollectionChangedEventArgs> collectionChanges1 = new List<NotifyCollectionChangedEventArgs>();
                List<NotifyCollectionChangedEventArgs> collectionChanges2 = new List<NotifyCollectionChangedEventArgs>();

                dictionary1.CollectionChanged += (s, e) => collectionChanges1.Add(e);
                dictionary2.CollectionChanged += (s, e) => collectionChanges2.Add(e);

                appInstance1.wire.SubModel(dictionary1);

                DateTime date1 = DateTime.Now;
                DateTime date2 = DateTime.Now;

                dictionary1.Add("key1", "test1");
                dictionary1.Add("key2", "test2");

                dictionary2.Add("key1", "test3");
                dictionary2.Add("key2", "test4");

                await Task.Delay(5000);

                appInstance1.dataChanges.Clear();
                appInstance2.dataChanges.Clear();
                collectionChanges1.Clear();
                collectionChanges2.Clear();

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 10;
                Assert.True(await appInstance1.wire.WaitForSynced(TimeSpan.FromMinutes(1)));
                Assert.True(await appInstance2.wire.WaitForSynced(TimeSpan.FromMinutes(1)));

                await Task.Delay(5000);

                appInstance2.wire.PutModel(dictionary2);

                appInstance2.app.Config.DatabaseMaxConcurrentSyncWrites = 10;
                Assert.True(await appInstance2.wire.WaitForSynced(TimeSpan.FromMinutes(1)));

                await Task.Delay(5000);

                Assert.Equal("test3", dictionary1["key1"]);
                Assert.Equal("test4", dictionary1["key2"]);

                Assert.Equal("test3", dictionary2["key1"]);
                Assert.Equal("test4", dictionary2["key2"]);

                Assert.Equal(2, collectionChanges1.Count);
                Assert.Contains(collectionChanges1, i =>
                {
                    return
                        i.Action == NotifyCollectionChangedAction.Replace &&
                        i.NewStartingIndex == 0 &&
                        i.NewItems?.Count == 1 &&
                        KeyValuePair.Create("key1", "test3").Equals(i.NewItems?[0]) &&
                        i.OldStartingIndex == 0 &&
                        i.OldItems?.Count == 1 &&
                        KeyValuePair.Create("key1", "test1").Equals(i.OldItems?[0]);
                });
                Assert.Contains(collectionChanges1, i =>
                {
                    return
                        i.Action == NotifyCollectionChangedAction.Replace &&
                        i.NewStartingIndex == 1 &&
                        i.NewItems?.Count == 1 &&
                        KeyValuePair.Create("key2", "test4").Equals(i.NewItems?[0]) &&
                        i.OldStartingIndex == 1 &&
                        i.OldItems?.Count == 1 &&
                        KeyValuePair.Create("key2", "test2").Equals(i.OldItems?[0]);
                });

                Assert.Empty(collectionChanges2);

                Assert.Equal(4, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "key1");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "key2");

                Assert.Equal(8, appInstance2.dataChanges.Count);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "key1");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "key2");

                appInstance1.app.Dispose();
                appInstance2.app.Dispose();
            });
        }

        [Fact]
        public async void Cascade()
        {
            await Helpers.CleanTest(nameof(FirebaseDictionaryTest), nameof(Cascade), async generator =>
            {
                var appInstance1 = await generator(null);
                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 0;
                appInstance1.wire.Start();

                var dictionary1 = new FirebaseDictionary<Couple>();

                List<NotifyCollectionChangedEventArgs> collectionChanges1 = new List<NotifyCollectionChangedEventArgs>();

                dictionary1.CollectionChanged += (s, e) =>
                {
                    collectionChanges1.Add(e);
                };

                appInstance1.wire.SubModel(dictionary1);

                var couple1 = new Couple();
                var person1 = new Person();

                DateTime date = DateTime.Now;
                person1.FirstName = "John";
                person1.LastName = "Doe";
                person1.Birthdate = date;

                couple1.FirstName = "Lara";
                couple1.LastName = "Croft";
                couple1.Birthdate = date;

                couple1.Partner = person1;

                dictionary1.Add("key1", couple1);

                await Task.Delay(5000);

                Assert.Equal(1, collectionChanges1.Count);
                Assert.Contains(collectionChanges1, i =>
                {
                    return
                        i.Action == NotifyCollectionChangedAction.Add &&
                        i.NewStartingIndex == 0 &&
                        i.NewItems?.Count == 1 &&
                        ((KeyValuePair<string, Couple>?)i.NewItems?[0])?.Key == "key1" &&
                        ((KeyValuePair<string, Couple>?)i.NewItems?[0])?.Value.FirstName == "Lara" &&
                        ((KeyValuePair<string, Couple>?)i.NewItems?[0])?.Value.LastName == "Croft" &&
                        ((KeyValuePair<string, Couple>?)i.NewItems?[0])?.Value.Birthdate == date &&
                        ((KeyValuePair<string, Couple>?)i.NewItems?[0])?.Value.Partner.FirstName == "John" &&
                        ((KeyValuePair<string, Couple>?)i.NewItems?[0])?.Value.Partner.LastName == "Doe" &&
                        ((KeyValuePair<string, Couple>?)i.NewItems?[0])?.Value.Partner.Birthdate == date &&
                        i.OldStartingIndex == -1 &&
                        i.OldItems?.Count == null;
                });
                collectionChanges1.Clear();

                Assert.Equal(14, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "key1");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key1" && i.Path[1] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key1" && i.Path[1] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key1" && i.Path[1] == "birthdate");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key1" && i.Path[1] == "partner");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 3 && i.Path[0] == "key1" && i.Path[1] == "partner" && i.Path[2] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 3 && i.Path[0] == "key1" && i.Path[1] == "partner" && i.Path[2] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 3 && i.Path[0] == "key1" && i.Path[1] == "partner" && i.Path[2] == "birthdate");
                appInstance1.dataChanges.Clear();

                appInstance1.app.Config.DatabaseMaxConcurrentSyncWrites = 10;
                Assert.True(await appInstance1.wire.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal("Lara", dictionary1["key1"].FirstName);
                Assert.Equal("Croft", dictionary1["key1"].LastName);
                Assert.Equal(date, dictionary1["key1"].Birthdate);
                Assert.Equal("John", dictionary1["key1"].Partner.FirstName);
                Assert.Equal("Doe", dictionary1["key1"].Partner.LastName);
                Assert.Equal(date, dictionary1["key1"].Partner.Birthdate);

                Assert.Empty(collectionChanges1);

                Assert.Equal(6, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key1" && i.Path[1] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key1" && i.Path[1] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key1" && i.Path[1] == "birthdate");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 3 && i.Path[0] == "key1" && i.Path[1] == "partner" && i.Path[2] == "first_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 3 && i.Path[0] == "key1" && i.Path[1] == "partner" && i.Path[2] == "last_name");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 3 && i.Path[0] == "key1" && i.Path[1] == "partner" && i.Path[2] == "birthdate");
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();
            });
        }

        [Fact]
        public async void Nullable()
        {
            await Helpers.CleanTest(nameof(FirebaseDictionaryTest), nameof(Nullable), async generator =>
            {
                var appInstance1 = await generator(new string[] { "part1" });
                appInstance1.wire.Start();
                var dictionary1 = new FirebaseDictionary<string>();
                List<NotifyCollectionChangedEventArgs> collectionChanges1 = new List<NotifyCollectionChangedEventArgs>();
                dictionary1.CollectionChanged += (s, e) =>
                {
                    collectionChanges1.Add(e);
                };

                appInstance1.wire.SubModel(dictionary1);

                dictionary1.Add("key1", "test1");
                dictionary1.Add("key2", "test2");

                Assert.True(await dictionary1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                await Task.Delay(5000);

                appInstance1.dataChanges.Clear();
                collectionChanges1.Clear();

                Assert.False(dictionary1.IsNull());
                Assert.True(dictionary1.SetNull());
                Assert.False(dictionary1.SetNull());
                Assert.True(dictionary1.IsNull());

                Assert.True(await dictionary1.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal(1, collectionChanges1.Count);
                Assert.Contains(collectionChanges1, i =>
                {
                    return
                        i.Action == NotifyCollectionChangedAction.Reset &&
                        i.NewStartingIndex == -1 &&
                        i.NewItems?.Count == null &&
                        i.OldStartingIndex == -1 &&
                        i.OldItems?.Count == null;
                });
                collectionChanges1.Clear();

                Assert.Equal(6, appInstance1.dataChanges.Count);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "key1");
                Assert.Contains(appInstance1.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "key2");
                appInstance1.dataChanges.Clear();

                appInstance1.app.Dispose();

                var appInstance2 = await generator(new string[] { "part2" });
                appInstance2.wire.Start();
                var dictionary2 = new FirebaseDictionary<Person>();
                List<NotifyCollectionChangedEventArgs> collectionChanges2 = new List<NotifyCollectionChangedEventArgs>();
                dictionary2.CollectionChanged += (s, e) =>
                {
                    collectionChanges2.Add(e);
                };

                appInstance2.wire.SubModel(dictionary2);

                var person1 = new Person();
                var person2 = new Person();

                DateTime date = DateTime.Now;

                person1.FirstName = "John";
                person1.LastName = "Doe";
                person1.Birthdate = date;

                person2.FirstName = "Lara";
                person2.LastName = "Croft";
                person2.Birthdate = date;

                dictionary2.Add("key1", person1);
                dictionary2.Add("key2", person2);

                Assert.True(await dictionary2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                await Task.Delay(5000);

                appInstance2.dataChanges.Clear();
                collectionChanges2.Clear();

                Assert.False(dictionary2.IsNull());
                Assert.True(dictionary2.SetNull());
                Assert.False(dictionary2.SetNull());
                Assert.True(dictionary2.IsNull());

                Assert.True(await dictionary2.RealtimeInstance.WaitForSynced(TimeSpan.FromMinutes(1)));

                Assert.Equal(1, collectionChanges2.Count);
                Assert.Contains(collectionChanges2, i =>
                {
                    return
                        i.Action == NotifyCollectionChangedAction.Reset &&
                        i.NewStartingIndex == -1 &&
                        i.NewItems?.Count == null &&
                        i.OldStartingIndex == -1 &&
                        i.OldItems?.Count == null;
                });
                collectionChanges2.Clear();

                Assert.Equal(20, appInstance2.dataChanges.Count);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 0);
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "key1");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key1" && i.Path[1] == "first_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key1" && i.Path[1] == "last_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key1" && i.Path[1] == "birthdate");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 1 && i.Path[0] == "key2");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key2" && i.Path[1] == "first_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key2" && i.Path[1] == "last_name");
                Assert.Contains(appInstance2.dataChanges, i => i.Path.Length == 2 && i.Path[0] == "key2" && i.Path[1] == "birthdate");
                appInstance2.dataChanges.Clear();

                appInstance2.app.Dispose();
            });
        }
    }
}
