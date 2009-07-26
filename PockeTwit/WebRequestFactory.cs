using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace PockeTwit
{
    class WebRequestFactory
    {
        public static HttpWebRequest CreateHttpRequest(string url)
        {
            return CreateHttpRequest(new Uri(url));
        }

        public static HttpWebRequest CreateHttpRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            if (!string.IsNullOrEmpty(ClientSettings.ProxyServer))
            {
                WebProxy proxy = new WebProxy(ClientSettings.ProxyServer, ClientSettings.ProxyPort);
                proxy.BypassProxyOnLocal = true;
                proxy.Credentials = CredentialCache.DefaultCredentials;
                request.Proxy = proxy;
            }
            return request;
        }
    }
}
