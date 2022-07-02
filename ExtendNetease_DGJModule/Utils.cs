using System;

namespace ExtendNetease_DGJModule
{
    public static class Utils
    {
        public static DateTime UnixTime2DateTime(int unixTimeStamp)
            => DateTimeOffset.FromUnixTimeSeconds(unixTimeStamp).DateTime.ToLocalTime();

        public static DateTime UnixTime2DateTime(long unixTimeStamp)
            => DateTimeOffset.FromUnixTimeMilliseconds(unixTimeStamp).DateTime.ToLocalTime();

        public static int DateTime2UnixTimeSeconds(DateTime time)
            => (int)new DateTimeOffset(time).ToUnixTimeSeconds();

        public static long DateTime2UnixTimeMillseconds(DateTime time)
            => new DateTimeOffset(time).ToUnixTimeMilliseconds();
    }
}
