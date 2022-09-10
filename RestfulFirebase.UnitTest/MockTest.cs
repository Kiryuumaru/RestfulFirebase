using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using RestfulFirebase.Authentication;
using RestfulFirebase.Authentication.Exceptions;
using RestfulFirebase.Authentication.Requests;
using RestfulFirebase.CloudFirestore.Requests;
using RestfulFirebase.Common.Requests;
using Xunit;

namespace RestfulFirebase.UnitTest
{
    public class MockTest
    {
        [Fact]
        public async void Test1()
        {
            FirebaseConfig config = Helpers.GetFirebaseConfig();

            FirebaseUser user;


            Assert.True(true);
        }
    }
}