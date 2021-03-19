namespace RestfulFirebase.Database.Streaming
{
    internal enum FirebaseServerEventType
    {
        Put,

        Patch,

        KeepAlive,

        Cancel,

        AuthRevoked
    }
}
