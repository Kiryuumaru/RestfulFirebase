namespace RestfulFirebase.RealtimeDatabase.Streaming;

internal enum ServerEventType
{
    Put,

    Patch,

    KeepAlive,

    Cancel,

    AuthRevoked
}
