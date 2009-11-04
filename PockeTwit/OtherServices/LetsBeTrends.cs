using System;

using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Collections;

namespace PockeTwit.OtherServices
{
    class LetsBeTrends
    {
        private const string API_currentTrends = "http://www.letsbetrends.com/api/current_trends";
        private const string API_getTrend = "http://www.letsbetrends.com/api/get_trend?name={0}";

        public static ArrayList GetCurrentTrends()
        {
            var urlToCall = API_currentTrends;
            var response = ExecuteGetCommand(urlToCall);
            if (!string.IsNullOrEmpty(response))
            {
                var list = (System.Collections.Hashtable)JSON.JsonDecode(response);
                return (ArrayList)list["trends"]; 
            }
            return null;
        }

        public static string GetTrend(string originalText)
        {
            if (originalText.Length < 1)
            {
                return "No trend to get";
            }
            var encodedText = System.Web.HttpUtility.UrlEncode(originalText);
            var urlToCall = string.Format(API_getTrend, encodedText);
            var response = ExecuteGetCommand(urlToCall);
            if (!string.IsNullOrEmpty(response))
            {
                var list = (System.Collections.Hashtable)JSON.JsonDecode(response);
                Hashtable ht = (Hashtable)list["description"];
                return (string)ht["text"];
            }
            return "No information found";
        }

        private static string ExecuteGetCommand(string url)
        {
            try
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
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                GlobalEventHandler.CallShowErrorMessage("Communications Error");
            }

            return null;
        }

    }
}
