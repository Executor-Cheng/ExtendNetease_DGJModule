using System.Net;
using System.Net.Http;

namespace ExtendNetease_DGJModule.Clients
{
    public class HttpClientv2 : HttpClient
    {
        public CookieContainer Cookies { get; }

        public HttpClientv2() : this(new HttpClientHandler())
        {

        }

        public HttpClientv2(HttpMessageHandler handler) : this(handler, true)
        {

        }

        public HttpClientv2(HttpMessageHandler handler, bool disposeHandler) : base(handler, disposeHandler)
        {
            if (handler is HttpClientHandler clientHandler)
            {
                Cookies = clientHandler.CookieContainer;
            }
        }
    }
}
