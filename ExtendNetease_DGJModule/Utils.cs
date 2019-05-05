using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExtendNetease_DGJModule
{
    public static class Utils
    {
        public static DateTime UnixTimeStamp2DateTime(int unixTimeStamp)
            => UnixTimeStamp2DateTime(unixTimeStamp * 1000L);

        public static DateTime UnixTimeStamp2DateTime(long unixTimeStamp)
        {
            return new DateTime(1970, 1, 1).AddMilliseconds(unixTimeStamp).ToLocalTime();
        }
    }
}
