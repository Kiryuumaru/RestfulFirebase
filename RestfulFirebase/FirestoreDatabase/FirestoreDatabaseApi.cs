

namespace RestfulFirebase.Api;

/// <summary>
/// Provides firebase cloud firestore database implementations.
/// </summary>
public static partial class FirestoreDatabase
{
    internal const string FirestoreDatabaseV1Endpoint = "https://firestore.googleapis.com/v1";
    internal const string FirestoreDatabaseDocumentsEndpoint = "projects/{0}/databases/{1}/documents{2}";
}
