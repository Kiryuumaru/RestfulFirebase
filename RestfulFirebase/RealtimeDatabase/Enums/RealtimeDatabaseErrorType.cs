namespace RestfulFirebase.RealtimeDatabase.Enums;

/// <summary>
/// The type of realtime database exception. 
/// </summary>
public enum RealtimeDatabaseErrorType
{
    /// <summary>
    /// Occurs when the request is malformed.
    /// </summary>
    BadRequestException,

    /// <summary>
    /// Occurs when there`s an internal server error.
    /// </summary>
    InternalServerErrorException,

    /// <summary>
    /// Occurs when the specified Realtime Database was not found.
    /// </summary>
    NotFoundException,

    /// <summary>
    /// Occurs when the request exceeds the database plan limits.
    /// </summary>
    PaymentRequiredException,

    /// <summary>
    /// Occurs when the request's specified ETag value in the if-match header did not match the server's value.
    /// </summary>
    PreconditionFailedException,

    /// <summary>
    /// Occurs when the specified Firebase Realtime Database is temporarily unavailable, which means the request was not attempted.
    /// </summary>
    ServiceUnavailableException,

    /// <summary>
    /// Occurs when the request is not authorized by database rules.
    /// </summary>
    UnauthorizedException,

    /// <summary>
    /// Occurs when there`s an unidentified exception.
    /// </summary>
    UndefinedException,
} 
