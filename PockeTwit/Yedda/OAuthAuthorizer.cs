//
// OAuth framework for TweetStation and PockeTwit
//
// Author;
//   Miguel de Icaza (miguel@gnome.org)
// Adapted for PockeTwit by;
//   Roel van den Brand (roelvandenbrand@gmail.com)
//
// Possible optimizations:
//   Instead of sorting every time, keep things sorted
//   Reuse the same dictionary, update the values
//
// Copyright 2010 Miguel de Icaza
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Net;
using System.Web;
using System.Linq;
using System.Security.Cryptography;
using OpenNETCF.Security.Cryptography;
using System.IO;
using ICSettings;
using System.Windows.Forms;

namespace OAuth
{
    /// <summary>
    /// OAuth authorising methods.
    /// </summary>
    public class OAuthAuthorizer
    {
        private static DataContract.Authorisation.OAuth _config = ICSettings.OAuthSettings.GetSettings("Twitter");

        public string RequestToken;
        public string RequestTokenSecret;
        public string AuthorizationToken;
        public string AuthorizationVerifier;
        public string AccessToken;
        public string AccessTokenSecret;
        public string AccessScreenname;
        public long AccessId;

        public OAuthAuthorizer()
        {

        }

        public DataContract.Authorisation.OAuth CurrentConfig
        {
            get
            {
                return _config;
            }
        }

        static Random random = new Random();
        static DateTime UnixBaseTime = new DateTime(1970, 1, 1);

        // 16-byte lower-case or digit string
        static string MakeNonce()
        {
            var ret = new char[16];
            for (int i = 0; i < ret.Length; i++)
            {
                int n = random.Next(35);
                if (n < 10)
                    ret[i] = (char)(n + '0');
                else
                    ret[i] = (char)(n - 10 + 'a');
            }
            return new string(ret);
        }

        static string MakeTimestamp()
        {
            return ((long)(DateTime.UtcNow - UnixBaseTime).TotalSeconds).ToString();
        }

        // Makes an OAuth signature out of the HTTP method, the base URI and the headers
        static string MakeSignature(string method, string base_uri, Dictionary<string, string> headers)
        {
            //string.Join ("%26", items.ToArray ());
            List<KeyValuePair<string, string>> KVP = new List<KeyValuePair<string, string>>(headers);
            KVP.Sort((p1, p2) =>
            {
                int c = p1.Key.CompareTo(p2.Key);
                if (c == 0)
                {
                    string p1v = p1.Value;
                    string p2v = p2.Value;
                    if (p1v == null)
                        p1v = "";
                    if (p2v == null)
                        p2v = "";
                    return p1v.CompareTo(p2v);
                }
                else
                    return c;
            }
            );
            List<string> lijst = new List<string>();
            foreach (KeyValuePair<string, string> p in KVP)
            {
                lijst.Add(p.Key + "%3D" + OAuth.PercentEncode(p.Value));
            }
            return method + "&" + OAuth.PercentEncode(base_uri) + "&" + string.Join("%26", lijst.ToArray());
        }

        static string MakeSigningKey(string consumerSecret, string oauthTokenSecret)
        {
            return OAuth.PercentEncode(consumerSecret) + "&" + (oauthTokenSecret != null ? OAuth.PercentEncode(oauthTokenSecret) : "");
        }

        static string MakeOAuthSignature(string compositeSigningKey, string signatureBase)
        {
            using (HMACSHA1 crypto = new HMACSHA1())
            {
                byte[] bkey = Encoding.ASCII.GetBytes(compositeSigningKey);
                // Keys longer than 60 characters aparently do not work with OpenNetCF...
                if (bkey.Length > 60)
                {
                    bkey = (new System.Security.Cryptography.SHA1CryptoServiceProvider()).ComputeHash(bkey);
                }

                crypto.Key = bkey;
                string hash = Convert.ToBase64String(crypto.ComputeHash(Encoding.ASCII.GetBytes(signatureBase)));
                crypto.Clear();

                return hash;
            }
        }

        static string HeadersToOAuth(Dictionary<string, string> headers)
        {
            List<string> lijst = new List<string>();
            foreach (string key in headers.Keys)
            {
                string value;
                if (headers.TryGetValue(key, out value))
                {
                    lijst.Add(String.Format("{0}=\"{1}\"", key, value));
                }
            }
            return "OAuth realm=\"http://api.twitter.com/\", " + String.Join(",", lijst.ToArray());
        }

        protected string HeadersToQueryString(Dictionary<string, string> headers, string signature)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string key in headers.Keys)
            {
                string value;
                if (headers.TryGetValue(key, out value))

                sb.AppendFormat("{0}={1}", key, value);  
                sb.Append("&");
            }
            sb.Append("oauth_signature=");
            sb.Append(signature);

            return sb.ToString();
        }


        public bool AcquireRequestToken()
        {
            var headers = new Dictionary<string, string>() {
				{ "oauth_callback", OAuth.PercentEncode (_config.Callback) },
				{ "oauth_consumer_key", _config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_version", "1.0" }};

            string signature = MakeSignature("GET", _config.RequestTokenUrl, headers);
            string compositeSigningKey = MakeSigningKey(_config.ConsumerSecret, null);
            string oauth_signature = MakeOAuthSignature(compositeSigningKey, signature);

            //Original code giving the same error as the code below the quoted code.

            //var wc = new WebClient ();
            //headers.Add ("oauth_signature", OAuth.PercentEncode (oauth_signature));
            //wc.Headers [HttpRequestHeader.Authorization.ToString()] = HeadersToOAuth (headers);

            //try {
            //    var result = HttpUtility.ParseQueryString(wc.UploadString(new Uri(_config.RequestTokenUrl), ""));

            //    if (result ["oauth_callback_confirmed"] != null){
            //        RequestToken = result ["oauth_token"];
            //        RequestTokenSecret = result ["oauth_token_secret"];

            //        return true;
            //    }
            //} catch (Exception e) {
            //    Console.WriteLine (e);
            //    // fallthrough for errors
            //}

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_config.AccessTokenUrl);
            headers.Add("oauth_signature", OAuth.PercentEncode(oauth_signature));
            request.Method = "GET";
            request.Headers.Add(HttpRequestHeader.Authorization.ToString(), HeadersToOAuth(headers));
            request.Timeout = 30000;
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                Stream resp = response.GetResponseStream();

                StreamReader oReader = new StreamReader(resp, Encoding.ASCII);

                string r = oReader.ReadToEnd();
                oReader.Close();
                response.Close();
                var result = HttpUtility.ParseQueryString(r);

                if (result["oauth_token"] != null)
                {
                    RequestToken = result["oauth_token"];
                    RequestTokenSecret = result["oauth_token_secret"];
                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // fallthrough for errors
                if (response != null)
                    response.Close();
            }

            return false;
        }

        // Invoked after the user has authorized us
        public bool AcquireAccessToken()
        {
            var headers = new Dictionary<string, string>() {
				{ "oauth_consumer_key", _config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_version", "1.0" }};

            headers.Add("oauth_token", AuthorizationToken);
            headers.Add("oauth_verifier", AuthorizationVerifier);

            string signature = MakeSignature("GET", _config.AccessTokenUrl, headers);
            string compositeSigningKey = MakeSigningKey(_config.ConsumerSecret, RequestTokenSecret);
            string oauth_signature = MakeOAuthSignature(compositeSigningKey, signature);


            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_config.AccessTokenUrl);
            headers.Add("oauth_signature", OAuth.PercentEncode(oauth_signature));
            request.Method = "GET";
            request.Headers.Add(HttpRequestHeader.Authorization.ToString(), HeadersToOAuth(headers));
            request.Timeout = 30000;
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)request.GetResponse();
                Stream resp = response.GetResponseStream();


                StreamReader oReader = new StreamReader(resp, Encoding.ASCII);

                string r = oReader.ReadToEnd();
                oReader.Close();
                response.Close();
                var result = HttpUtility.ParseQueryString(r);

                if (result["oauth_token"] != null)
                {
                    AccessToken = result["oauth_token"];
                    AccessTokenSecret = result["oauth_token_secret"];
                    AccessScreenname = result["screen_name"];
                    AccessId = Int64.Parse(result["user_id"]);

                    return true;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // fallthrough for errors
                if(response != null)
                    response.Close();
            }

            return false;
        }





        /// <summary>
        /// 
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool AcquireXAuth(string username, string password)
        {
            var headers = new Dictionary<string, string>() {
				{ "oauth_consumer_key", _config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_version", "1.0" }};

            headers.Add("x_auth_mode", "client_auth");
            headers.Add("x_auth_password", OAuth.PercentEncode(password));
            headers.Add("x_auth_username", OAuth.PercentEncode(username));

            string signature = MakeSignature("GET", _config.XAuthUrl, headers);
            string compositeSigningKey = MakeSigningKey(_config.ConsumerSecret, null);
            string oauth_signature = MakeOAuthSignature(compositeSigningKey, signature);
            string querystring = HeadersToQueryString(headers, OAuth.PercentEncode(oauth_signature));

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(_config.XAuthUrl + "?" + querystring);
            //headers.Add("oauth_signature", OAuth.PercentEncode(oauth_signature));

            request.UserAgent = "PockeTwit";
            request.Method = "GET";
            //request.Headers.Add(HttpRequestHeader.Authorization.ToString(), HeadersToOAuth(headers));
            //string body = "x_auth_username={0}&x_auth_password={1}&x_auth_mode=client_auth";
            //byte[] message = Encoding.UTF8.GetBytes(OAuth.PercentEncode(string.Format(body,username,password)));
            //request.ContentLength = message.Length; 

            request.Timeout = 30000;
            //HttpWebResponse response = null;
            try
            {
               //using (Stream requestStream = request.GetRequestStream())
               //{
               //      requestStream.Write(message, 0, message.Length);
               //     requestStream.Flush();
               //     requestStream.Close();

                    using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                    {
                        Stream resp = response.GetResponseStream();

                        StreamReader oReader = new StreamReader(resp, Encoding.ASCII);

                        string r = oReader.ReadToEnd();
                        MessageBox.Show(r);

                        oReader.Close();
                        response.Close();
                        var result = HttpUtility.ParseQueryString(r);

                        if (result["oauth_token"] != null)
                        {
                            AccessToken = result["oauth_token"];
                            AccessTokenSecret = result["oauth_token_secret"];
                            return true;
                        }
                    }
                //}
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                PockeTwit.GlobalEventHandler.LogCommError(e);
                //MessageBox.Show(e.StackTrace);
                // fallthrough for errors
                throw e;
            }

            return false;
        }

        // 
        // Assign the result to the Authorization header, like this:
        // request.Headers [HttpRequestHeader.Authorization] = AuthorizeRequest (...)
        //
        public static string AuthorizeRequest(string oauthToken, string oauthTokenSecret, string method, Uri uri, string data)
        {
            var headers = new Dictionary<string, string>() {
				{ "oauth_consumer_key", _config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_token", oauthToken },
				{ "oauth_version", "1.0" }};
            var signatureHeaders = new Dictionary<string, string>(headers);

            // Add the data and URL query string to the copy of the headers for computing the signature
            if (data != null && data != "")
            {
                var parsed = HttpUtility.ParseQueryString(data);
                foreach (string k in parsed.Keys)
                {
                    signatureHeaders.Add(k, OAuth.PercentEncode(parsed[k]));
                }
            }

            var nvc = HttpUtility.ParseQueryString(uri.Query);
            foreach (string key in nvc)
            {
                if (key != null)
                    signatureHeaders.Add(key, OAuth.PercentEncode(nvc[key]));
            }

            string signature = MakeSignature(method, uri.GetLeftPart(UriPartial.Path), signatureHeaders);
            string compositeSigningKey = MakeSigningKey(_config.ConsumerSecret, oauthTokenSecret);
            string oauth_signature = MakeOAuthSignature(compositeSigningKey, signature);

            headers.Add("oauth_signature", OAuth.PercentEncode(oauth_signature));

            return HeadersToOAuth(headers);
        }

        //
        // Used to authorize an HTTP request going to TwitPic
        //
        public static void AuthorizeEcho(HttpWebRequest wc, string oauthToken, string oauthTokenSecret)
        {
            AuthorizeEcho(wc, oauthToken, oauthTokenSecret, Yedda.Twitter.OutputFormatType.JSON);
        }
        public static void AuthorizeEcho(HttpWebRequest wc, string oauthToken, string oauthTokenSecret, Yedda.Twitter.OutputFormatType OutputFormatType)
        {
            var headers = new Dictionary<string, string>() {
                //{ "realm", "http://api.twitter.com/" },
                { "oauth_consumer_key", _config.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_token", oauthToken },
				{ "oauth_version", "1.0" }
			};
            string signurl = String.Format("https://api.twitter.com/1/account/verify_credentials.{0}", Yedda.Twitter.GetFormatTypeString(OutputFormatType));
            // The signature is not done against the *actual* url, it is done against the verify_credentials.json one 
            string signature = MakeSignature("GET", signurl, headers);
            string compositeSigningKey = MakeSigningKey(_config.ConsumerSecret, oauthTokenSecret);
            string oauth_signature = MakeOAuthSignature(compositeSigningKey, signature);

            headers.Add("oauth_signature", OAuth.PercentEncode(oauth_signature));

            wc.Headers.Add("X-Verify-Credentials-Authorization", HeadersToOAuth(headers));
            wc.Headers.Add("X-Auth-Service-Provider", signurl);
        }
    }

    public static class OAuth
    {

        // 
        // This url encoder is different than regular Url encoding found in .NET 
        // as it is used to compute the signature based on a url.   Every document
        // on the web omits this little detail leading to wasting everyone's time.
        //
        // This has got to be one of the lamest specs and requirements ever produced
        //
        public static string PercentEncode(string s)
        {
            var sb = new StringBuilder();

            foreach (byte c in Encoding.UTF8.GetBytes(s))
            {
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == '-' || c == '_' || c == '.' || c == '~')
                    sb.Append((char)c);
                else
                {
                    sb.AppendFormat(CultureInfo.CurrentCulture, "%{0:X2}", c);
                }
            }
            return sb.ToString();
        }

       
    }
}


