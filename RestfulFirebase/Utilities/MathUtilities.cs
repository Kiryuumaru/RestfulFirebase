using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulFirebase.Utilities
{
    /// <summary>
    /// Provides convenient math utilities.
    /// </summary>
    public static class MathUtilities
    {
        /// <summary>
        /// Converts unsigned number to other base system.
        /// </summary>
        /// <param name="number">
        /// The unsigned number to convert.
        /// </param>
        /// <param name="baseSystem">
        /// The base system to convert to.
        /// </param>
        /// <returns>
        /// The unsigned array convertion representation of the <paramref name="number"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throws when the provided <paramref name="baseSystem"/> parameter is below 1.
        /// </exception>
        public static uint[] ToUnsignedArbitraryBaseSystem(ulong number, uint baseSystem)
        {
            if (baseSystem < 2) throw new ArgumentOutOfRangeException("Provided base system is below 1.");
            var baseArr = new List<uint>();
            while (number >= baseSystem)
            {
                var ans = number / baseSystem;
                var remainder = number % baseSystem;
                number = ans;
                baseArr.Add((uint)remainder);
            }
            baseArr.Add((uint)number);
            baseArr.Reverse();
            return baseArr.ToArray();
        }

        /// <summary>
        /// Converts unsigned arbitrary base system to normal base system.
        /// </summary>
        /// <param name="arbitraryBaseNumber">
        /// The arbitrary base system to convert.
        /// </param>
        /// <param name="baseSystem">
        /// The base number of the provided <paramref name="arbitraryBaseNumber"/> parameter.
        /// </param>
        /// <returns>
        /// The unsigned number representation of the <paramref name="arbitraryBaseNumber"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throws when the provided <paramref name="baseSystem"/> parameter is below 1 or is outside on a number from the provided <paramref name="arbitraryBaseNumber"/> parameter.
        /// </exception>
        public static ulong ToUnsignedNormalBaseSystem(uint[] arbitraryBaseNumber, uint baseSystem)
        {
            if (baseSystem < 2) throw new ArgumentOutOfRangeException("Provided base system is below 1.");
            if (arbitraryBaseNumber.Any(i => i >= baseSystem)) throw new ArgumentOutOfRangeException("Number has greater value than base number system.");
            ulong value = 0;
            var reverse = arbitraryBaseNumber.Reverse().ToArray();
            for (int i = 0; i < arbitraryBaseNumber.Length; i++)
            {
                value += (ulong)(reverse[i] * Math.Pow(baseSystem, i));
            }
            return value;
        }

        /// <summary>
        /// Converts number to other base system.
        /// </summary>
        /// <param name="number">
        /// The number to convert.
        /// </param>
        /// <param name="baseSystem">
        /// The base system to convert to.
        /// </param>
        /// <returns>
        /// The unsigned array convertion representation of the <paramref name="number"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throws when the provided <paramref name="baseSystem"/> parameter is below 1.
        /// </exception>
        public static uint[] ToSignedArbitraryBaseSystem(long number, uint baseSystem)
        {
            var num = ToUnsignedArbitraryBaseSystem((ulong)Math.Abs(number), baseSystem);
            var newNum = new uint[num.Length + 1];
            Array.Copy(num, 0, newNum, 1, num.Length);
            newNum[0] = number < 0 ? baseSystem - 1 : 0;
            return newNum;
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
        /// The unsigned number representation of the <paramref name="arbitraryBaseNumber"/> parameter.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Throws when the provided <paramref name="baseSystem"/> parameter is below 1 or is outside on a number from the provided <paramref name="arbitraryBaseNumber"/> parameter.
        /// </exception>
        public static long ToSignedNormalBaseSystem(uint[] arbitraryBaseNumber, uint baseSystem)
        {
            bool isNegative;
            if (arbitraryBaseNumber[0] == 0) isNegative = false;
            else if (arbitraryBaseNumber[0] == baseSystem - 1) isNegative = true;
            else throw new ArgumentException("Provided arbitrary base number is not a signed number.");
            var num = (long)ToUnsignedNormalBaseSystem(arbitraryBaseNumber.Skip(1).ToArray(), baseSystem);
            return isNegative ? -num : num;
        }

        /// <summary>
        /// Calculates variance from the collection of numbers.
        /// </summary>
        /// <param name="data">
        /// The collection of data to calculate.
        /// </param>
        /// <returns>
        /// The calculated variance of the provided <paramref name="data"/> parameter.
        /// </returns>
        public static double Variance(IEnumerable<double> data)
        {
            double mean = data.Average();
            double sum = 0;
            foreach (double d in data) sum += Math.Pow(d - mean, 2);
            return sum / (data.Count() - 1);
        }

        /// <summary>
        /// Calculates standard deviation from the collection of numbers.
        /// </summary>
        /// <param name="data">
        /// The collection of data to calculate.
        /// </param>
        /// <returns>
        /// The calculated standard deviation of the provided <paramref name="data"/> parameter.
        /// </returns>
        public static double StandardDeviation(IEnumerable<double> data)
        {
            double mean = data.Average();
            double sum = 0;
            foreach (double d in data) sum += Math.Pow(d - mean, 2);
            return Math.Pow(sum / data.Count(), 0.5);
        }
    }
}
