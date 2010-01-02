using System;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;

namespace PockeTwit.OtherServices.GoogleSpell
{
	/// <summary>
	/// Summary description for SpellCheck.
	/// </summary>
	public class SpellCheck
	{
		// defaults
		protected static string QueryFormat = "lang={0}&hl={0}";		
		protected static string GoogleBaseUri = "http://www.google.com/tbproxy/spell";
		// languages must be sorted
		protected static string[] Languages = new string[] {"da","de","en","es","fi","fr","it","nl","pl","pt","sv"};
		protected static CultureInfo DefaultCulture = new CultureInfo("en");

		private static CultureInfo[] _supportedLanguage;
		private static object _spellLock = new object();
		
		private SpellCheck()
		{ }

		public static CultureInfo[] SupportedLanguages
		{
			get
			{
				lock(_spellLock)
				{
					if (_supportedLanguage == null)
					{
						// lazy load
						_supportedLanguage = new CultureInfo[Languages.Length];
						for(int x = 0; x < Languages.Length; x++)
						{
							_supportedLanguage[x] = new CultureInfo(Languages[x]);
						}		
					}
				}
				return _supportedLanguage;
			}
		}
		
		/// <summary>Sends a spell check request to the google speck checking api.</summary>
		/// <param name="request">The google spell check request</param>
		/// <returns>An instance of SpellResult containing the corrections</returns>
		public static SpellResult Check(SpellRequest request)
		{
			return SpellCheck.Check(request, DefaultCulture, false);
		}

		/// <summary>Sends a spell check request to the google speck checking api.</summary>
		/// <param name="request">The google spell check request</param>
		/// <param name="language">The language to spell check with</param>
		/// <returns>An instance of SpellResult containing the corrections</returns>
		public static SpellResult Check(SpellRequest request, CultureInfo language)
		{
			return SpellCheck.Check(request, language, false);
		}

		/// <summary>Sends a spell check request to the google speck checking api.</summary>
		/// <param name="request">The google spell check request</param>
		/// <param name="encryptRequest">Encrypte request by using https</param>
		/// <returns>An instance of SpellResult containing the corrections</returns>
		public static SpellResult Check(SpellRequest request, bool encryptRequest)
		{
			return SpellCheck.Check(request, DefaultCulture, encryptRequest);
		}

		/// <summary>Sends a spell check request to the google speck checking api.</summary>
		/// <param name="request">The google spell check request</param>
		/// <param name="language">The language to spell check with</param>
		/// <param name="encryptRequest">Encrypte request by using https</param>
		/// <returns>An instance of SpellResult containing the corrections</returns>
		public static SpellResult Check(SpellRequest request, CultureInfo language, bool encryptRequest)
		{
			if (request == null)
				throw new ArgumentNullException("request");
			if (language == null)
				throw new ArgumentNullException("language");
			if(Array.BinarySearch(Languages, language.TwoLetterISOLanguageName) < 0)
				throw new ArgumentException("The specified language is not supported.", "language");
				
			string uriString = BuildUri(language, encryptRequest);
			
			string requestxml = request.ToString();
			Debug.WriteLine(requestxml);

			byte [] buffer = Encoding.UTF8.GetBytes(requestxml);
			
            //WebClient webClient = new WebClient();
            //webClient.Headers.Add("Content-Type", "text/xml");
			
            //byte[] response = webClient.UploadData(uriString, "POST", buffer);

            //string resultXML = Encoding.UTF8.GetString(response);
            //Debug.WriteLine(resultXML);	

           
            HttpWebRequest httpRequest = (HttpWebRequest)HttpWebRequest.Create(uriString);

            httpRequest.Method = "POST";
            httpRequest.ContentType = "text/xml";
            httpRequest.ContentLength = buffer.Length;

            System.IO.Stream postStream = httpRequest.GetRequestStream();
            postStream.Write(buffer, 0, buffer.Length);
            postStream.Close();

            HttpWebResponse httpResponse = (HttpWebResponse)httpRequest.GetResponse();

            System.IO.Stream dataStream = httpResponse.GetResponseStream();

            System.IO.StreamReader streamReader = new System.IO.StreamReader(dataStream);

            String data = streamReader.ReadToEnd();

            streamReader.Close();
            httpResponse.Close();


            SpellResult result = SpellResult.Load(data);

            //SpellResult result = SpellResult.Load(resultXML);

            return result;
  
		}

		private static string BuildUri(CultureInfo language, bool encryptRequest)
		{
            //Uri googleUri = new Uri(GoogleBaseUri);
            
            ////UriBuilder googleUri = new UriBuilder(GoogleBaseUri);

            //googleUri.Scheme = encryptRequest ? "https" : "http";
            //googleUri.Query = string.Format(QueryFormat, language.TwoLetterISOLanguageName);
            //return googleUri.ToString();

            return "https://www.google.com/tbproxy/spell?lang=en";
		}
	}
}
