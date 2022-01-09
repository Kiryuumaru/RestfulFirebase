using ObservableHelpers;
using ObservableHelpers.Utilities;
using RestfulFirebase;
using RestfulFirebase.Database.Models;
using RestfulFirebase.Database.Realtime;
using RestfulFirebase.Local;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Playground
{
    public static class Helpers
    {
        public static Task<(Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>> generator, Action dispose)> AuthenticatedTestApp(string testName, string factName)
        {
            return RestfulFirebase.Test.Helpers.AuthenticatedTestApp(nameof(Playground), testName, factName);
        }

        public static Task CleanTest(
            string testName,
            string factName,
            Func<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>, Task> test)
        {
            return RestfulFirebase.Test.Helpers.CleanTest(nameof(Playground), testName, factName, test);
        }

        public static Task CleanTest(
            string testName,
            string factName,
            Action<Func<string[]?, Task<(RestfulFirebaseApp app, RealtimeWire wire, List<DataChangesEventArgs> dataChanges)>>> test)
        {
            return RestfulFirebase.Test.Helpers.CleanTest(nameof(Playground), testName, factName, test);
        }
    }

    public class Mock
    {
        public class MockObj : FirebaseObject
        {
            public DateTime Started
            {
                get => GetFirebasePropertyWithKey<DateTime>("started");
                internal set => SetFirebasePropertyWithKey(value, "started");
            }

            public DateTime? Ended
            {
                get => GetFirebasePropertyWithKey<DateTime?>("ended");
                internal set => SetFirebasePropertyWithKey(value, "ended");
            }
        }

        [Fact]
        public async void Mockk()
        {
            await Helpers.CleanTest(nameof(Mock), nameof(Mockk), generator =>
            {
                MockObj obj = new MockObj();

                RWLock rwLock = new RWLock(System.Threading.LockRecursionPolicy.SupportsRecursion);

                ObservableProperty<DateTime?> ssss = new ObservableProperty<DateTime?>();
                FirebaseProperty<DateTime?> sss = new FirebaseProperty<DateTime?>();

                if (sss.Value == null)
                {

                }
                if (sss.GetValue() == null)
                {

                }
                if (ssss.Value == null)
                {

                }
                if (ssss.GetValue() == null)
                {

                }

                Assert.True(true);
            });
        }
    }
}
