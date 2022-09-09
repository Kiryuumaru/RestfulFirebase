using System;
using System.Collections.Generic;
using System.Linq;

namespace RestfulFirebase.Utilities;

/// <summary>
/// Provides convenient math utilities.
/// </summary>
internal static class NumberSerializer
{
    /// <summary>
    /// Converts number to other base system.
    /// </summary>
    /// <param name="number">
    /// The unsigned number to convert.
    /// </param>
    /// <param name="baseSystem">
    /// The base system to convert to.
    /// </param>
    /// <returns>
    /// The array convertion representation of the <paramref name="number"/> parameter.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throws when the provided <paramref name="baseSystem"/> parameter is below 2.
    /// </exception>
    public static int[] ToArbitraryBaseSystem(long number, int baseSystem)
    {
        if (baseSystem < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(baseSystem));
        }

        List<int> baseArr = new();

        if (number < 0)
        {
            while (number <= -baseSystem)
            {
                long ans = number / baseSystem;
                long remainder = number % baseSystem;
                number = ans;
                baseArr.Insert(0, (int)Math.Abs(remainder));
            }
            baseArr.Insert(0, (int)Math.Abs(number));
            baseArr.Insert(0, -1);
        }
        else
        {
            while (number >= baseSystem)
            {
                long ans = number / baseSystem;
                long remainder = number % baseSystem;
                number = ans;
                baseArr.Insert(0, (int)remainder);
            }
            baseArr.Insert(0, (int)number);
        }

        return baseArr.ToArray();
    }

    /// <summary>
    /// Converts arbitrary base system to normal base system.
    /// </summary>
    /// <param name="arbitraryBaseNumber">
    /// The arbitrary base system to convert.
    /// </param>
    /// <param name="baseSystem">
    /// The base number of the provided <paramref name="arbitraryBaseNumber"/> parameter.
    /// </param>
    /// <returns>
    /// The number representation of the <paramref name="arbitraryBaseNumber"/> parameter.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// <paramref name="arbitraryBaseNumber"/> is empty.
    /// </exception>
    /// <exception cref="ArgumentNullException">
    /// <paramref name="arbitraryBaseNumber"/> is a null reference.
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Throws when the provided <paramref name="baseSystem"/> parameter is below 2 or is outside on a number from the provided <paramref name="arbitraryBaseNumber"/> parameter.
    /// </exception>
    public static long ToNormalBaseSystem(int[] arbitraryBaseNumber, int baseSystem)
    {
        if (baseSystem < 2)
        {
            throw new ArgumentOutOfRangeException(nameof(baseSystem));
        }
        if (arbitraryBaseNumber == null)
        {
            ArgumentNullException.ThrowIfNull(arbitraryBaseNumber);
        }
        if (arbitraryBaseNumber.Length == 0)
        {
            throw new ArgumentException(nameof(arbitraryBaseNumber) + " is empty.");
        }

        bool isNegative = arbitraryBaseNumber[0] < 0;
        int floorLoop = isNegative ? 1 : 0;
        long value = 0;

        for (int i = floorLoop; i < arbitraryBaseNumber.Length; i++)
        {
            if (arbitraryBaseNumber[i] >= baseSystem)
            {
                throw new ArgumentOutOfRangeException(nameof(arbitraryBaseNumber));
            }
            value += (long)(arbitraryBaseNumber[i] * Math.Pow(baseSystem, arbitraryBaseNumber.Length - i - 1));
        }

        return isNegative ? -value : value;
    }
}
