using System;
using System.Collections.Generic;
using System.Text;

namespace RestfulFirebase.Extensions
{
    public static class UIDFactory
    {
        private static readonly char[] PushChars = Encoding.UTF8.GetChars(Encoding.UTF8.GetBytes(Utils.Base64Charset));
        private static readonly DateTimeOffset Epoch = new DateTimeOffset(1970, 1, 1, 0, 0, 0, 0, TimeSpan.Zero);
        private static readonly Random random = new Random();
        private static readonly byte[] lastRandChars = new byte[12];

        private static long lastPushTime;

        public static string GenerateUID(int length = 10, string charset = Utils.Base64Charset)
        {
            string id = "";
            for (int i = 0; i < length; i++)
            {
                id += charset[random.Next(charset.Length)];
            }
            return id;
        }

        public static string GenerateSafeUID()
        {
            var id = new StringBuilder(20);
            var now = (long)(DateTimeOffset.Now - Epoch).TotalMilliseconds;
            var duplicateTime = now == lastPushTime;
            lastPushTime = now;

            var timeStampChars = new char[8];
            for (int i = 7; i >= 0; i--)
            {
                var index = (int)(now % PushChars.Length);
                timeStampChars[i] = PushChars[index];
                now = (long)Math.Floor((double)now / PushChars.Length);
            }

            if (now != 0)
            {
                throw new Exception("We should have converted the entire timestamp.");
            }

            id.Append(timeStampChars);

            if (!duplicateTime)
            {
                for (int i = 0; i < 12; i++)
                {
                    lastRandChars[i] = (byte)random.Next(0, PushChars.Length);
                }
            }
            else
            {
                var lastIndex = 11;
                for (; lastIndex >= 0 && lastRandChars[lastIndex] == PushChars.Length - 1; lastIndex--)
                {
                    lastRandChars[lastIndex] = 0;
                }

                lastRandChars[lastIndex]++;
            }

            for (int i = 0; i < 12; i++)
            {
                id.Append(PushChars[lastRandChars[i]]);
            }

            if (id.Length != 20)
            {
                throw new Exception("Length should be 20.");
            }

            return id.ToString();
        }
    }
}
