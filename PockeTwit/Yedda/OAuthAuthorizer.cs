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


namespace OAuth
{
    //
    // Configuration information for an OAuth client
    //
    public static class OAuthConfig
    {
        // keys, callbacks
        public static string ConsumerKey = "eejwjEguCY80lgPYhp1ag";
        public static string Callback = string.Empty; //oob?
        public static string ConsumerSecret = "hhaFilap5NWXtdXeTVCnOl5H2lEzK8hyWqDQaVamc";
        public static string TwitPicKey = "5b976ad6e50575acef064fe98ae67bcc";
        public static string BitlyKey = string.Empty;
        public static string MobyPictureKey = "p0ck3tTTTTw";

        // Urls
        public static string RequestTokenUrl = "http://api.twitter.com/oauth/request_token";
        public static string AccessTokenUrl = "http://api.twitter.com/oauth/access_token";
        public static string AuthorizeUrl = "http://api.twitter.com/oauth/authorize";
        public static string VerifyUrl = "http://api.twitter.com/1/account/verify_credentials.json";

        public static string PosterousUrl = "https://posterous.com/api2/upload.format";
        public static string TwitPicUrl = "http://api.twitpic.com/2/upload.format";
        public static string MobyPictureUrl = "https://api.mobypicture.com/2.0/upload.format";
    }

    //
    // These static methods only require the access token:
    //    AuthorizeRequest
    //    AuthorizeTwitPic
    //
    public class OAuthAuthorizer
    {

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
            //using static config.
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
                    return p1.Value.CompareTo(p2.Value);
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
            return "OAuth " + String.Join(",", lijst.ToArray());
        }

        public bool AcquireRequestToken()
        {
            var headers = new Dictionary<string, string>() {
				{ "oauth_callback", OAuth.PercentEncode (OAuthConfig.Callback) },
				{ "oauth_consumer_key", OAuthConfig.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_version", "1.0" }};

            string signature = MakeSignature("POST", OAuthConfig.RequestTokenUrl, headers);
            string compositeSigningKey = MakeSigningKey(OAuthConfig.ConsumerSecret, null);
            string oauth_signature = MakeOAuthSignature(compositeSigningKey, signature);

            //Original code giving the same error as the code below the quoted code.

            //var wc = new WebClient ();
            //headers.Add ("oauth_signature", OAuth.PercentEncode (oauth_signature));
            //wc.Headers [HttpRequestHeader.Authorization.ToString()] = HeadersToOAuth (headers);

            //try {
            //    var result = HttpUtility.ParseQueryString(wc.UploadString(new Uri(OAuthConfig.RequestTokenUrl), ""));

            //    if (result ["oauth_callback_confirmed"] != null){
            //        RequestToken = result ["oauth_token"];
            //        RequestTokenSecret = result ["oauth_token_secret"];

            //        return true;
            //    }
            //} catch (Exception e) {
            //    Console.WriteLine (e);
            //    // fallthrough for errors
            //}

            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(OAuthConfig.RequestTokenUrl);
            headers.Add("oauth_signature", OAuth.PercentEncode(oauth_signature));
            request.Method = "POST";
            request.Headers.Add(HttpRequestHeader.Authorization.ToString(), HeadersToOAuth(headers));
            request.Timeout = 30000;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream resp = response.GetResponseStream();


                StreamReader oReader = new StreamReader(resp, Encoding.ASCII);

                string r = oReader.ReadToEnd();

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
            }

            return false;
        }

        // Invoked after the user has authorized us
        public bool AcquireAccessToken()
        {
            var headers = new Dictionary<string, string>() {
				{ "oauth_consumer_key", OAuthConfig.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_version", "1.0" }};
            var content = "";

            headers.Add("oauth_token", AuthorizationToken);
            headers.Add("oauth_verifier", AuthorizationVerifier);

            string signature = MakeSignature("GET", OAuthConfig.AccessTokenUrl, headers);
            string compositeSigningKey = MakeSigningKey(OAuthConfig.ConsumerSecret, RequestTokenSecret);
            string oauth_signature = MakeOAuthSignature(compositeSigningKey, signature);


            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(OAuthConfig.AccessTokenUrl);
            headers.Add("oauth_signature", OAuth.PercentEncode(oauth_signature));
            request.Method = "POST";
            request.Headers.Add(HttpRequestHeader.Authorization.ToString(), HeadersToOAuth(headers));
            request.Timeout = 30000;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                Stream resp = response.GetResponseStream();


                StreamReader oReader = new StreamReader(resp, Encoding.ASCII);

                string r = oReader.ReadToEnd();

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
				{ "oauth_consumer_key", OAuthConfig.ConsumerKey },
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
            string compositeSigningKey = MakeSigningKey(OAuthConfig.ConsumerSecret, oauthTokenSecret);
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
                { "oauth_consumer_key", OAuthConfig.ConsumerKey },
				{ "oauth_nonce", MakeNonce () },
				{ "oauth_signature_method", "HMAC-SHA1" },
				{ "oauth_timestamp", MakeTimestamp () },
				{ "oauth_token", oauthToken },
				{ "oauth_version", "1.0" }
			};
            string signurl = String.Format("https://api.twitter.com/1/account/verify_credentials.{0}", Yedda.Twitter.GetFormatTypeString(OutputFormatType));
            // The signature is not done against the *actual* url, it is done against the verify_credentials.json one 
            string signature = MakeSignature("GET", signurl, headers);
            string compositeSigningKey = MakeSigningKey(OAuthConfig.ConsumerSecret, oauthTokenSecret);
            string oauth_signature = MakeOAuthSignature(compositeSigningKey, signature);

            headers.Add("oauth_signature", OAuth.PercentEncode(oauth_signature));

            //Util.Log ("Headers: " + HeadersToOAuth (headers));
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


