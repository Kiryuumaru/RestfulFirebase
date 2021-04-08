using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Common.Models
{
    public struct SmallDateTime
    {
        private DateTime baseDateTime;

        public static readonly SmallDateTime MaxValue = new SmallDateTime(DateTime.MaxValue);
        public static readonly SmallDateTime MinValue = new SmallDateTime(0);

        public static SmallDateTime Now => new SmallDateTime(DateTime.Now);
        public static SmallDateTime Today => new SmallDateTime(DateTime.Today);
        public static SmallDateTime UtcNow => new SmallDateTime(DateTime.UtcNow);

        public SmallDateTime(DateTime baseDateTime)
        {
            this.baseDateTime = baseDateTime;
        }

        public SmallDateTime(long compressedDateTime)
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
            if (obj is SmallDateTime unix)  return this == unix;
            return false;
        }

        public override int GetHashCode()
        {
            return baseDateTime.GetHashCode();
        }

        public static SmallDateTime operator +(SmallDateTime d, TimeSpan t) => new SmallDateTime(d.baseDateTime + t);
        public static TimeSpan operator -(SmallDateTime d1, SmallDateTime d2) => d1.baseDateTime - d2.baseDateTime;
        public static SmallDateTime operator -(SmallDateTime d, TimeSpan t) => new SmallDateTime(d.baseDateTime - t);
        public static bool operator ==(SmallDateTime d1, SmallDateTime d2) => d1.baseDateTime == d2.baseDateTime;
        public static bool operator !=(SmallDateTime d1, SmallDateTime d2) => d1.baseDateTime != d2.baseDateTime;
        public static bool operator <(SmallDateTime t1, SmallDateTime t2) => t1.baseDateTime < t2.baseDateTime;
        public static bool operator >(SmallDateTime t1, SmallDateTime t2) => t1.baseDateTime > t2.baseDateTime;
        public static bool operator <=(SmallDateTime t1, SmallDateTime t2) => t1.baseDateTime <= t2.baseDateTime;
        public static bool operator >=(SmallDateTime t1, SmallDateTime t2) => t1.baseDateTime >= t2.baseDateTime;
    }
}
