using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;
using Yedda;
using OAuth;
using System.Windows.Forms;
using System.Security.Cryptography.X509Certificates;

namespace PockeTwit.MediaServices
{
    public class MobyPicture : PictureServiceBase
    {
        #region private properties
        private static volatile MobyPicture _instance;
        private static object syncRoot = new Object();

        private const string APPLICATION_NAME = "p0ck3tTTTTw";
        private const string API_URL = "http://api.mobypicture.com";

        private const string API_UPLOAD = "http://api.mobypicture.com/";
        private const string API_UPLOAD_V2 = "http://api.mobypicture.com/2.0/upload.xml"; // https gives a read failure for some reason
//        private const string API_UPLOAD_POST_v2 = "https://api.mobypicture.com/2.0/upload.xml";
        private const string API_GET_THUMB = "http://api.mobypicture.com/?s=medium&format=plain&k=" + APPLICATION_NAME;  //The extra / for directly sticking the image-id on.

        private const string API_ERROR_UPLOAD = "Unable to upload picture to MobyPicture.";
        private const string API_ERROR_DOWNLOAD = "Unable to download picture from MobyPicture.";

        private byte[] readBuffer;
        private Stream dataStream;
        private bool useAsyncCall = false;
        private Twitter.Account _account = null;

        #endregion

        #region private objects

        private System.Threading.Thread workerThread;
        private PicturePostObject workerPPO;

        #endregion

        #region constructor
        /// <summary>
        /// Private constructor for usage in singleton.
        /// </summary>
        private MobyPicture()
        {
            
            API_SAVE_TO_PATH = "\\ArtCache\\www.mobypicture.com\\";
            API_SERVICE_NAME = "MobyPicture";
            API_CAN_UPLOAD_GPS = true;
            API_CAN_UPLOAD_MESSAGE = false;
            API_URLLENGTH = 31;

            API_FILETYPES.Add(new MediaType("jpg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("jpeg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("gif", "image/gif", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("png", "image/png", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("bmp", "image/bmp", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("flv", "video/x-flv", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mpeg", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mkv", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("wmv", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mov", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("3gp", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mp4", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("avi", "", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mp3", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("wma", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("aac", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("aif", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("au", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("flac", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("ra", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("wav", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("ogg", "", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("amr", "", MediaTypeGroup.AUDIO));
        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static MobyPicture Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new MobyPicture();
                            _instance.HasEventHandlersSet = false;
                        }
                    }
                }
                return _instance;
            }
        }

        #endregion

        #region IPictureService Members

        /// <summary>
        /// Post a picture
        /// </summary>
        /// <param name="postData"></param>
        public override bool PostPicture(PicturePostObject postData, Twitter.Account account)
        {
            return DoPost(postData, account, true);
        }

        private bool DoPost(PicturePostObject postData, Twitter.Account account, bool successEvent)
        {
            #region Argument check

            this._account = account;

            //Check for empty path
            if (string.IsNullOrEmpty(postData.Filename))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }

            //Check for empty credentials
            if (string.IsNullOrEmpty(account.OAuth_token) ||
                string.IsNullOrEmpty(account.OAuth_token_secret))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
            }

            #endregion

            using (System.IO.FileStream file = new FileStream(postData.Filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    #region async
                    //if (postData.UseAsync)
                    //{
                    //    workerPPO = (PicturePostObject) postData.Clone();
                    //    workerPPO.PictureData = incoming;

                    //    if (workerThread == null)
                    //    {
                    //        workerThread = new System.Threading.Thread(new System.Threading.ThreadStart(ProcessUpload));
                    //        workerThread.Name = "PictureUpload";
                    //        workerThread.Start();
                    //    }
                    //    else
                    //    {
                    //        OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, string.Empty, "A request is already running."));
                    //    }
                    //}
                    //else
                    //{
                    #endregion
                    //use sync.

                    postData.PictureStream = file;
                    XmlDocument uploadResult = UploadPicture(API_UPLOAD_V2, postData);
                    //if (File.Exists("mobyresult.txt"))
                    //{
                    //    File.Delete("mobyresult.txt");
                    //}
                    //XmlTextWriter xm = new XmlTextWriter("mobyresult.txt", Encoding.UTF8);
                    //uploadResult.Save(xm);
                    //XmlDocument uploadResult = new XmlDocument();                    
                    //uploadResult.Load("mobyresult.txt"); 

                    if (uploadResult == null) // occurs in the event of an error
                    {
                        return false;
                    }
                    if (successEvent)
                    {
                        string URL = uploadResult.SelectSingleNode("//mediaurl").InnerText;
                        postData.URL = URL;
                        OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, postData.Filename));
                    }
                }
                catch (Exception ex)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Fetch a picture.
        /// </summary>
        /// <param name="pictureURL"></param>
        public override void FetchPicture(string pictureURL, Twitter.Account account)
        {
            #region Argument check

            //Need a url to read from.
            if (string.IsNullOrEmpty(pictureURL))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            }

            #endregion

            _account = account;

            try
            {
                workerPPO = new PicturePostObject();
                workerPPO.Message = pictureURL;

                if (workerThread == null)
                {
                    workerThread = new System.Threading.Thread(new System.Threading.ThreadStart(ProcessDownload));
                    workerThread.Name = "PictureUpload";
                    workerThread.Start();
                }
                else
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, string.Empty, "A request is already running."));
                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            } 
        }

        /// <summary>
        /// Check whether the service can fetch an URL
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public override bool CanFetchUrl(string URL)
        {
            return (SiteTester_1(URL) || SiteTester_2(URL));
        }

        private bool SiteTester_1(string URL)
        {
            const string siteMarker = "mobypicture";
            const string notAllowedInUrl = "user";

            string url = URL.ToLower();
            return (url.IndexOf(siteMarker) >= 0 && url.IndexOf(notAllowedInUrl) < 0);
        }

        private bool SiteTester_2(string URL)
        {
            const string siteMarker = "moby.to";
            //const string notAllowedInUrl = "user";

            string url = URL.ToLower();
            return (url.IndexOf(siteMarker) >= 0);
        }

        #endregion

        #region thread implementation

        private void ProcessDownload()
        {
            try
            {
                string pictureURL = workerPPO.Message;
                int imageIdStartIndex = pictureURL.LastIndexOf('/') + 1;
                string imageID = pictureURL.Substring(imageIdStartIndex, pictureURL.Length - imageIdStartIndex);

                if (!useAsyncCall)
                {
                    string resultFileName = RetrievePicture(pictureURL);
                    if (!string.IsNullOrEmpty(resultFileName))
                    {
                        OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, resultFileName, ""));
                    }
                }
                else
                {
                    RetrievePictureAsync(pictureURL);
                }
            }
            catch (Exception)
            {
                //No need to throw, postPicture throws event.        
                //OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", "Unable to download picture, try again later."));
            }
            workerThread = null;
        }

        //private void ProcessUpload()
        //{
        //    try
        //    {
        //        postData.PictureStream = file;
        //        XmlDocument uploadResult = UploadPicture(API_UPLOAD, postData, account);

        //        if (uploadResult == null) // occurs in the event of an error
        //            return false;
        //        if (successEvent)
        //        {
        //            string URL = uploadResult.SelectSingleNode("//url").InnerText;
        //            OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, postData.Filename));
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
        //    }
        //    workerThread = null;
        //}

        #endregion

        #region private methods
        /// <summary>
        /// Upload a picture
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ppo"></param>
        /// <returns></returns>
        private XmlDocument UploadPicture(string url, PicturePostObject ppo)
        {
            try
            {
                HttpWebRequest request = WebRequestFactory.CreateHttpRequest(url);
                request.Timeout = 20000;
                request.AllowWriteStreamBuffering = false; // don't want to buffer 3MB files, thanks
                request.AllowAutoRedirect = false;
                request.Headers.Add("Action", "upload");

                Multipart contents = new Multipart();
                contents.UploadPart += new Multipart.UploadPartEvent(contents_UploadPart);
                contents.Add("key", APPLICATION_NAME);

                if (!string.IsNullOrEmpty(ppo.Message))
                {
                    contents.Add("message", ppo.Message);
                    string hashTags = FindHashTags(ppo.Message, ",", 32);
                    if (!string.IsNullOrEmpty(hashTags))
                    {
                        contents.Add("tags", hashTags);
                    }
                }
                else
                {
                    contents.Add("message", "");
                }

                if (!string.IsNullOrEmpty(ppo.Lat) && !string.IsNullOrEmpty(ppo.Lon))
                {
                    contents.Add("latlong", string.Format("{0},{1}", ppo.Lat, ppo.Lon));
                }

                contents.Add("media", ppo.PictureStream, Path.GetFileName(ppo.Filename), ppo.ContentType);

                OAuthAuthorizer.AuthorizeEcho(request, _account.OAuth_token, _account.OAuth_token_secret);

                return contents.UploadXML(request);
            }
            catch (WebException ex)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, API_ERROR_UPLOAD, String.Format("Received response {0} from server ({1})", ex.Status, ex.Message)));
                return null;
            }
            catch (Exception e)
            {
                //Socket exception 10054 could occur when sending large files.

                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return null;
            }
        }

        private void contents_UploadPart(object sender, long bytesSent, long bytesTotal)
        {
            OnUploadPart(new PictureServiceEventArgs((int)bytesSent, (int)bytesSent, (int)bytesTotal));
        }


/*        private string HeadersToString(Dictionary<string, string> headers)
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
            return String.Join(",", lijst.ToArray());
        }*/
    

        /// <summary>
        /// Use a imageId to retrieve and save a thumbnail to the device.
        /// </summary>
        /// <param name="imageId">Id for the image</param>
        /// <returns></returns>
        private string RetrievePicture(string pictureURL)
        {
            try
            {
                HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(API_GET_THUMB + "&t=" + pictureURL);
                myRequest.Method = "GET";
                String pictureFileName = String.Empty;

                using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (dataStream = response.GetResponseStream())
                    {
                        int totalSize = 0;
                        readBuffer = new byte[PT_READ_BUFFER_SIZE];
                        pictureFileName = GetPicturePath(pictureURL);
                        int totalResponseSize = (int)response.ContentLength;

                        int responseSize = dataStream.Read(readBuffer, 0, PT_READ_BUFFER_SIZE);
                        totalSize = responseSize;
                        OnDownloadPart(new PictureServiceEventArgs(responseSize, responseSize, totalResponseSize));
                        while (responseSize > 0)
                        {
                            base.SavePicture(pictureFileName, readBuffer, responseSize);
                            try
                            {
                                totalSize += responseSize;
                                responseSize = dataStream.Read(readBuffer, 0, PT_READ_BUFFER_SIZE);
                                OnDownloadPart(new PictureServiceEventArgs(responseSize, totalSize, totalResponseSize));
                            }
                            catch
                            {
                                responseSize = 0;
                            }
                            System.Threading.Thread.Sleep(100);
                        }
                        dataStream.Close();
                    }
                    response.Close();
                }

                return pictureFileName;
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return string.Empty;
            }
        }

        /// <summary>
        /// Use a imageId to retrieve and save a thumbnail to the device.
        /// </summary>
        /// <param name="imageId">Id for the image</param>
        /// <returns></returns>
        private string RetrievePictureAsync(string pictureURL)
        {
            try
            {
                HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(API_GET_THUMB);
                myRequest.Method = "GET";
                String pictureFileName = String.Empty;

                using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (dataStream = response.GetResponseStream())
                    {
                        readBuffer = new byte[PT_READ_BUFFER_SIZE];
                        pictureFileName = GetPicturePath(pictureURL);

                        AsyncStateData state = new AsyncStateData();

                        state.dataHolder = readBuffer;
                        state.dataStream = dataStream;
                        state.fileName = pictureFileName;
                        state.totalBytesToDownload = (int)response.ContentLength;

                        dataStream.BeginRead(readBuffer, 0, PT_READ_BUFFER_SIZE, new System.AsyncCallback(DownloadPartFinished), state);

                    }
                    response.Close();
                }

                return pictureFileName;
            }
            catch (Exception)
            {            
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return string.Empty;
            }
        }

        /// <summary>
        /// Asynchronised callback method
        /// </summary>
        /// <param name="ar"></param>
        private void DownloadPartFinished(IAsyncResult ar)
        {
            AsyncStateData state = (AsyncStateData)ar.AsyncState;

            try
            {
                int len = state.dataStream.EndRead(ar);
                
                state.bytesRead = len;
                state.totalBytesRead += state.bytesRead;

                bool saveSucces = SavePicture(state.fileName, state.dataHolder, PT_READ_BUFFER_SIZE);
                if (!saveSucces)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
                }

                OnDownloadPart(new PictureServiceEventArgs(state.bytesRead, state.totalBytesRead, state.totalBytesToDownload));

                if (ar.IsCompleted)
                {
                    //OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, state.fileName, ""));
                }

                dataStream.BeginRead(readBuffer, 0, PT_READ_BUFFER_SIZE, new System.AsyncCallback(DownloadPartFinished), state);
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            }
        }


        #endregion
    }
}
