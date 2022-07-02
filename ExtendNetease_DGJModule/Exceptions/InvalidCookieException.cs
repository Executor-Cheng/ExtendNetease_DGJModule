using System;

namespace ExtendNetease_DGJModule.Exceptions
{
    public sealed class InvalidCookieException : Exception
    {
        public InvalidCookieException() : base("给定的Cookie无效.") { }

        public InvalidCookieException(string message) : base(message) { }

        public InvalidCookieException(string message, Exception innerException) : base(message, innerException) { }
    }
}
