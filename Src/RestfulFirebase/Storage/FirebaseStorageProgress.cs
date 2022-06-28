namespace RestfulFirebase.Storage;

/// <summary>
/// The firebase storage file progress report
/// </summary>
public class FirebaseStorageProgress
{
    internal FirebaseStorageProgress(long position, long length)
    {
        Position = position;
        Length = length;
        Percentage = (int)((position / (double)length) * 100);
    }

    /// <summary>
    /// The total length of the progress.
    /// </summary>
    public long Length { get; private set; }

    /// <summary>
    /// Percentage of the progress.
    /// </summary>
    public int Percentage { get; private set; }

    /// <summary>
    /// The position length of the progress.
    /// </summary>
    public long Position { get; private set; }
}
