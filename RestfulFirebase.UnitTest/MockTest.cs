using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Requests;
using RestfulFirebase.CloudFirestore.Query;
using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.Common.Requests;
using RestfulFirebase.FirestoreDatabase;
using RestfulFirebase.FirestoreDatabase.Abstraction;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Xunit;

namespace RestfulFirebase.UnitTest
{
    public class Test11
    {
        public string? Val1 { get; set; }
    }

    public class Coordinates : IGeoPoint
    {
        public double Latitude { get; set; }

        public double Longitude { get; set; }
    }

    public class AllType
    {
        public string? Type1 { get; set; }

        public double Type2 { get; set; }

        public bool Type3 { get; set; }

        public Dictionary<string, string?>? Type4 { get; set; }

        public List<string?>? Type5 { get; set; }

        public object? Type6 { get; set; }

        public DateTimeOffset? Type7 { get; set; }

        public Coordinates? Type8 { get; set; }

        public DocumentReference? Type9 { get; set; }
    }

    public class MockTest
    {
        [Fact]
        public async void Test1()
        {
            FirebaseConfig config = Helpers.GetFirebaseConfig();

            FirebaseUser user;

            Document<AllType>? allType = await Api.FirestoreDatabase.GetDocument(new GetDocumentRequest<AllType>()
            {
                Config = config,
                Reference = Api.FirestoreDatabase.Database()
                    .Collection("public")
                    .Document("allType")
            });

            await Api.FirestoreDatabase.PatchDocument(new PatchDocumentRequest<AllType>()
            {
                Config = config,
                Model = allType.Model,
                Reference = Api.FirestoreDatabase.Database()
                    .Collection("public")
                    .Document("allType1")
            });

            Assert.True(true);
        }
    }
}