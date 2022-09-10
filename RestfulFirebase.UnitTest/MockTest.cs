using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Requests;
using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.Common.Requests;
using RestfulFirebase.FirestoreDatabase;
using System.Text.Json.Serialization;
using Xunit;

namespace RestfulFirebase.UnitTest
{
    public class Person
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        [JsonPropertyName("value1")]
        public string? Others { get; set; }
    }

    public class MockTest
    {
        [Fact]
        public async void Test1()
        {
            FirebaseConfig config = Helpers.GetFirebaseConfig();

            FirebaseUser user;

            Document<Person>? person = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<Person>()
            {
                Config = config,
                Reference = Api.FirestoreDatabase.Database()
                    .Collection("public")
                    .Document("sample")
            });

            await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<Person>()
            {
                Config = config,
                Model = person.Model,
                Reference = Api.FirestoreDatabase.Database()
                    .Collection("public")
                    .Document("sample1")
            });

            Assert.True(true);
        }
    }
}