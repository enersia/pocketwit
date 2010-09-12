using System;
using System.IO;
using System.Net;


namespace PockeTwit.OtherServices.TextShrinkers
{
    class _140it : ITextShrinker
    {
        private const string API = "http://140it.com/api/shrink?char_max={1}&text={0}";
        public string GetShortenedText(string originalText)
        {
            return GetShortenedText(originalText, 140);
        }

        public string GetShortenedText(string originalText, int charMax)
        {
            if(originalText.Length <= charMax)
            {
                return originalText;
            }
            var encodedText = System.Web.HttpUtility.UrlEncode(originalText);
            var urlToCall = string.Format(API, encodedText, charMax);
            var response = ExecuteGetCommand(urlToCall);
            if (!string.IsNullOrEmpty(response))
            {
                var list = (System.Collections.Hashtable) JSON.JsonDecode(response);
                return (string) list["new"];
            }
            return originalText;
        }

        public string ServiceName()
        {
            return "140it";
        }

        public string ServiceDescription()
        {
            return "Will abbreviate words to reduce the number of letters.";
        }

        private static string ExecuteGetCommand(string url)
        {
            var request = WebRequestFactory.CreateHttpRequest(url);
            using (var httpResponse = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = httpResponse.GetResponseStream())
                {
                    using (var reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
        }

        
    }
}