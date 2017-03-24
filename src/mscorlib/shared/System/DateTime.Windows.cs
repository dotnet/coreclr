using System.Diagnostics.Contracts;

namespace System
{
    public partial struct DateTime
    {
        public static unsafe DateTime UtcNow
        {
            get
            {
                Contract.Ensures(Contract.Result<DateTime>().Kind == DateTimeKind.Utc);
                // following code is tuned for speed. Don't change it without running benchmark.
                long ticks = Interop.Kernel32.GetSystemTimeAsFileTime();

                return new DateTime(((UInt64)(ticks + FileTimeOffset)) | KindUtc);
            }
        }
    }
}
