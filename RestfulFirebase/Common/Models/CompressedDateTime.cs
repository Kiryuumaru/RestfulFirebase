using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public struct CompressedDateTime
    {
        private DateTime baseDateTime;

        public static readonly CompressedDateTime MaxValue = new CompressedDateTime(DateTime.MaxValue);
        public static readonly CompressedDateTime MinValue = new CompressedDateTime(0);

        public static CompressedDateTime Now => new CompressedDateTime(DateTime.Now);
        public static CompressedDateTime Today => new CompressedDateTime(DateTime.Today);
        public static CompressedDateTime UtcNow => new CompressedDateTime(DateTime.UtcNow);

        public CompressedDateTime(DateTime baseDateTime)
        {
            this.baseDateTime = baseDateTime;
        }

        public CompressedDateTime(long compressedDateTime)
        {
            this.baseDateTime = new DateTime(((long)compressedDateTime * 10000L) + 631139040000000000L);
        }

        public DateTime GetBaseDateTime()
        {
            return baseDateTime;
        }

        public long GetCompressedTime()
        {
            return (baseDateTime.Ticks - 631139040000000000L) / 10000L;
        }

        public override bool Equals(object obj)
        {
            if (obj is CompressedDateTime unix)  return this == unix;
            return false;
        }

        public override int GetHashCode()
        {
            return baseDateTime.GetHashCode();
        }

        public static CompressedDateTime operator +(CompressedDateTime d, TimeSpan t) => new CompressedDateTime(d.baseDateTime + t);
        public static TimeSpan operator -(CompressedDateTime d1, CompressedDateTime d2) => d1.baseDateTime - d2.baseDateTime;
        public static CompressedDateTime operator -(CompressedDateTime d, TimeSpan t) => new CompressedDateTime(d.baseDateTime - t);
        public static bool operator ==(CompressedDateTime d1, CompressedDateTime d2) => d1.baseDateTime == d2.baseDateTime;
        public static bool operator !=(CompressedDateTime d1, CompressedDateTime d2) => d1.baseDateTime != d2.baseDateTime;
        public static bool operator <(CompressedDateTime t1, CompressedDateTime t2) => t1.baseDateTime < t2.baseDateTime;
        public static bool operator >(CompressedDateTime t1, CompressedDateTime t2) => t1.baseDateTime > t2.baseDateTime;
        public static bool operator <=(CompressedDateTime t1, CompressedDateTime t2) => t1.baseDateTime <= t2.baseDateTime;
        public static bool operator >=(CompressedDateTime t1, CompressedDateTime t2) => t1.baseDateTime >= t2.baseDateTime;
    }
}
