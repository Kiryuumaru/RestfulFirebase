using System;
using System.Threading;

namespace RestfulFirebase.Utilities;

/// <summary>
/// Provides <see cref="Random"/> class utilities.
/// </summary>
public static class RandomUtilities
{
    private static int seed = Environment.TickCount;

    private static ThreadLocal<Random> randomWrapper = new        (() => new Random(Interlocked.Increment(ref seed)));

    /// <summary>
    /// Gets a thread-safe <see cref="Random"/> instance.
    /// </summary>
    /// <returns>
    /// The thread-safe <see cref="Random"/> instance.
    /// </returns>
    public static Random GetThreadRandom()
    {
        return randomWrapper.Value;
    }
}
