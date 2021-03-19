using System.IO;
using System.Text;

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
            this.buffer = new byte[bufferSize];

            this.cachedData = string.Empty;
        }

        public override string ReadLine()
        {
            var currentString = this.TryGetNewLine();
            
            while (currentString == null)
            {
                var read = this.stream.Read(this.buffer, 0, this.bufferSize);
                var str = Encoding.UTF8.GetString(buffer, 0, read);

                cachedData += str;
                currentString = this.TryGetNewLine();
            }
            
            return currentString;
        }

        private string TryGetNewLine()
        {
            var newLine = cachedData.IndexOf('\n');

            if (newLine >= 0)
            {
                var r = cachedData.Substring(0, newLine + 1);
                this.cachedData = cachedData.Remove(0, r.Length);
                return r.Trim();
            }

            return null;
        }
    }
}
