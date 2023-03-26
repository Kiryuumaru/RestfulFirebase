using Xunit;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Enums;
using RestfulFirebase.Common.Models;
using System.Collections.Generic;
using System.Linq;
using RestfulFirebase.Common.Utilities;
using System.Threading.Tasks;
using System.IO;

namespace RestfulFirebase.UnitTest;

public class MockTest
{
    //[Fact]
    //public async void Test1()
    //{
    //    FirebaseApp app = Helpers.GetFirebaseApp();

    //    CollectionReference testCollectionReference = app.FirestoreDatabase
    //        .Collection("public")
    //        .Document(nameof(MockTest))
    //        .Collection(nameof(Test1));

    //    await FirestoreDatabaseTest.Cleanup(testCollectionReference);

    //    await testCollectionReference.PatchDocuments(new (string documentName, NestedType? model)[]
    //    {
    //        ("model1", NestedType.Filled1(app)),
    //        ("model2", NestedType.Filled2(app)),
    //        ("model3", NestedType.Filled1(app)),
    //        ("model4", NestedType.Filled1(app))
    //    });

    //    await Task.Delay(1);
    //}
}
