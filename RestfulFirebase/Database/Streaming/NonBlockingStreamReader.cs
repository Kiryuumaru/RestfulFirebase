using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace RestfulFirebase.Database.Streaming
{
    public class NonBlockingStreamReader : TextReader
    {
        private const int DefaultBufferSize = 16000;

        private readonly Stream stream;
        private readonly byte[] buffer;
        private readonly int bufferSize;

        private string cachedData;

        public NonBlockingStreamReader(Stream stream, int bufferSize = DefaultBufferSize)
        {
            this.stream = stream;
            this.bufferSize = bufferSize;
            buffer = new byte[bufferSize];

            cachedData = string.Empty;
        }

        public override async Task<string> ReadLineAsync()
        {
            var currentString = TryGetNewLine();

            while (currentString == null)
            {
                var read = await stream.ReadAsync(buffer, 0, bufferSize);
                var str = Encoding.UTF8.GetString(buffer, 0, read);

                cachedData += str;
                currentString = TryGetNewLine();
            }

            return currentString;
        }

        private string TryGetNewLine()
        {
            var newLine = cachedData.IndexOf('\n');

            if (newLine >= 0)
            {
                var r = cachedData.Substring(0, newLine + 1);
                cachedData = cachedData.Remove(0, r.Length);
                return r.Trim();
            }

            return null;
        }
    }
}
