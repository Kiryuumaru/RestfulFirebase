using Xunit;
using RestfulFirebase.FirestoreDatabase.References;
using RestfulFirebase.FirestoreDatabase.Transactions;
using RestfulFirebase.FirestoreDatabase.Queries;
using RestfulFirebase.FirestoreDatabase.Requests;
using RestfulFirebase.FirestoreDatabase.Models;
using RestfulFirebase.FirestoreDatabase.Enums;

namespace RestfulFirebase.UnitTest;

public class MockTest
{
    [Fact]
    public async void Test1()
    {
        FirebaseConfig config = Helpers.GetFirebaseConfig();

        CollectionReference testCollectionReference = Api.FirestoreDatabase
            .Collection("public")
            .Document(nameof(FirestoreDatabaseTest))
            .Collection(nameof(FirestoreDatabaseTest.ListDocumentsTest));

        Document<NormalMVVMModel>[] writeDocuments = testCollectionReference.CreateDocuments<NormalMVVMModel>(
            ($"test1", new() // a_c
            {
                Val1 = "a",
                Val2 = "c"
            }),
            ($"test2", new() // a_d
            {
                Val1 = "a",
                Val2 = "d"
            }),
            ($"test3", new() // b_e
            {
                Val1 = "b",
                Val2 = "e"
            }),
            ($"test4", new() // b_f
            {
                Val1 = "b",
                Val2 = "f"
            }),
            ($"test5", new() // b_g
            {
                Val1 = "b",
                Val2 = "g"
            }));

        // Order for query
        // a_d
        // a_c
        // b_g
        // b_f
        // b_e

        var writeTest1 = await Api.FirestoreDatabase.WriteDocument(new WriteDocumentRequest()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            PatchDocument = writeDocuments
        });
        await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            Document = writeDocuments
        });

        var sss = await Api.FirestoreDatabase.RunQuery(new RunQueryRequest<NormalMVVMModel>()
        {
            Config = config,
            JsonSerializerOptions = Helpers.JsonSerializerOptions,
            From = testCollectionReference,
            Where = FilterQuery.Builder.Create()
                .Field(nameof(NormalMVVMModel.Val1), FieldOperator.Equal, "b"),
            OrderBy = OrderByQuery.Builder.Create()
                .Descending(nameof(NormalMVVMModel.Val2)),
        });

        await FirestoreDatabaseTest.Cleanup(config, testCollectionReference);

        Assert.True(true);
    }
}
