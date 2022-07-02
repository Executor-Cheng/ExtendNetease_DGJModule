using System;

namespace ExtendNetease_DGJModule.Exceptions
{
    public class LoginFailedException : Exception
    {
        public string Reason { get; set; }

        public LoginFailedException(string reason) : base($"登录失败。{reason}")
        {
            Reason = reason;
        }
    }
}
