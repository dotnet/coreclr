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
                
                // For performance, use a private constructor that does not validate arguments.
                return new DateTime(((ulong)(Interop.Sys.GetSystemTimeAsTicks() + TicksTo1970)) | KindUtc);
            }
        }
    }
}
