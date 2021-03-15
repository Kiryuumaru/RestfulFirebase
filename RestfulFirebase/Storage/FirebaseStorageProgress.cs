namespace RestfulFirebase.Storage
{
    public class FirebaseStorageProgress
    {
        public FirebaseStorageProgress(long position, long length)
        {
            Position = position;
            Length = length;
            Percentage = (int)((position / (double)length) * 100);
        }

        public long Length
        {
            get;
            private set;
        }

        public int Percentage
        {
            get;
            private set;
        }

        public long Position
        {
            get;
            private set;
        }
    }
}
