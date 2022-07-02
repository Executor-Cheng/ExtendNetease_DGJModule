using Newtonsoft.Json.Linq;
using System;

namespace ExtendNetease_DGJModule.Exceptions
{
    public class UnknownResponseException : Exception
    {
        private const string DefaultMessage = "未知的服务器返回.";

        /// <summary>
        /// Gets the server responded message.
        /// </summary>
        public string? Response { get; }

        public UnknownResponseException() { }

        public UnknownResponseException(string? response) : this(response, DefaultMessage, null) { }

        public UnknownResponseException(string? response, string message) : this(response, message, null) { }

        public UnknownResponseException(string? response, Exception? innerException) : this(response, DefaultMessage, innerException) { }

        public UnknownResponseException(string? response, string message, Exception? innerException) : base(message, innerException)
            => Response = response;

        public UnknownResponseException(JToken node) : this(node.ToString(0)) { }

        public UnknownResponseException(JToken node, string? message) : this(node.ToString(0), message) { }
    }
}
