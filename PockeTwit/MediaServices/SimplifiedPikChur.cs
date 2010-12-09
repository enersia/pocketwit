using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;
using OAuth;
using Yedda;

namespace PockeTwit.MediaServices
{
    public class SimplifiedPikChur : PictureServiceBase
    {
        #region private properties
        private static volatile SimplifiedPikChur _instance;
        private static object syncRoot = new Object();

        private const string API_UPLOAD = "http://api.pikchur.com/simple/upload";
        //private const string API_SHOW_THUMB = "http://img.ly/show/large/";  //The extra / for directly sticking the image-id on.

        private const string API_ERROR_UPLOAD = "Unable to upload to Pikchur";
        private const string API_ERROR_DOWNLOAD = "Unable to download from Pikchur";
        private const string API_KEY = "fzC/xJKgGySRN82+UPYvDA";
        private const string API_ORIGIN_ID = "MjUx";

        private Twitter.Account account = null;
        #endregion

        #region private objects

        private System.Threading.Thread workerThread;
        private PicturePostObject workerPPO;

        #endregion

        #region constructor
        /// <summary>
        /// Private constructor for usage in singleton.
        /// </summary>
        private SimplifiedPikChur()
        {
            API_SAVE_TO_PATH = "\\ArtCache\\pikchur.com\\";
            API_SERVICE_NAME = "Pikchur";
            API_CAN_UPLOAD_GPS = false;
            API_CAN_UPLOAD_MESSAGE = false;
            API_URLLENGTH = 25;

            API_FILETYPES.Add(new MediaType("jpg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("jpeg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("gif", "image/gif", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("png", "image/png", MediaTypeGroup.PICTURE));
        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static SimplifiedPikChur Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new SimplifiedPikChur();
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
        /// Fetch a picture
        /// </summary>
        /// <param name="pictureURL"></param>
        public override void FetchPicture(string pictureURL, Twitter.Account account)
        {
            #region Argument check

            this.account = account;

            //Need a url to read from.
            if (string.IsNullOrEmpty(pictureURL))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_DOWNLOAD));
            }

            #endregion

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
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, "", "A request is already running."));
                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_DOWNLOAD));
            }
        }


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
            this.account = account;
            #region Argument check

            this.account = account;

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
                    //use sync.

                    postData.PictureStream = file;
                    XmlDocument uploadResult = UploadPicture(API_UPLOAD, postData, account);

                    if (uploadResult == null) // occurs in the event of an error
                        return false;
                    if (successEvent)
                    {
                        string URL = uploadResult.SelectSingleNode("//mediaurl").InnerText;
                        postData.URL = URL;
                        OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, postData.Filename));
                    }
                }
                catch (Exception /*ex*/)
                {
                    // could do with catching a SocketException here, so we can give more useful information to the user
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Test whether this service can fetch a picture.
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public override bool CanFetchUrl(string URL)
        {
            string[] siteMarkers = {
                "pikchur", "pk.gd", "mp.gd"
            };
            string url = URL.ToLower();
            foreach (string siteMarker in siteMarkers)
            {
                if(url.IndexOf(siteMarker) >= 0)
                    return true;
            }
            return false;
        }

        #endregion

        #region thread implementation

        private void ProcessDownload()
        {
            try
            {
                string pictureURL = workerPPO.Message;
                Uri url = new Uri(pictureURL);
                string resultFileName = "";
                if (url.Host.Equals("mp.gd", StringComparison.InvariantCultureIgnoreCase) || url.AbsolutePath.StartsWith("/v/", StringComparison.InvariantCultureIgnoreCase))
                {
                    // video
                    int imageIdStartIndex = pictureURL.LastIndexOf('/') + 1;
                    string imageID = pictureURL.Substring(imageIdStartIndex, pictureURL.Length - imageIdStartIndex);
                    resultFileName = RetrievePicture(imageID, MediaTypeGroup.VIDEO);
                }
                else
                {
                    int imageIdStartIndex = pictureURL.LastIndexOf('/') + 1;
                    string imageID = pictureURL.Substring(imageIdStartIndex, pictureURL.Length - imageIdStartIndex);
                    resultFileName = RetrievePicture(imageID, MediaTypeGroup.PICTURE);
                    // picture
                }




                 

                if (!string.IsNullOrEmpty(resultFileName))
                {
                    OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, resultFileName, string.Empty, pictureURL));
                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            }
            workerThread = null;
        }

        #endregion

        #region private methods

        /// <summary>
        /// Use a imageId to retrieve and save a thumbnail to the device.
        /// </summary>
        /// <param name="imageId">Id for the image</param>
        /// <returns></returns>
        private string RetrievePicture(string imageId, MediaTypeGroup type)
        {
            try
            {
                string url = "";
                if (type == MediaTypeGroup.VIDEO)
                {
                    url = string.Format("http://vid.pikchur.com/vid_{0}_l.jpg", imageId);
                }
                else
                {
                    url = string.Format("http://img.pikchur.com/pic_{0}_l.jpg", imageId);
                }

                HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(url);
                myRequest.Method = "GET";
                String pictureFileName = String.Empty;

                using (HttpWebResponse response = (HttpWebResponse)myRequest.GetResponse())
                {
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        int totalSize = 0;
                        int totalResponseSize = (int)response.ContentLength;
                        byte[] readBuffer = new byte[PT_READ_BUFFER_SIZE];
                        pictureFileName = GetPicturePath(imageId);

                        int responseSize = dataStream.Read(readBuffer, 0, PT_READ_BUFFER_SIZE);
                        totalSize = responseSize;
                        OnDownloadPart(new PictureServiceEventArgs(responseSize, totalSize, totalResponseSize));
                        while (responseSize > 0)
                        {
                            SavePicture(pictureFileName, readBuffer, responseSize);
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
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
                return string.Empty;
            }
        }


        /// <summary>
        /// Upload the picture
        /// </summary>
        /// <param name="url">URL to upload picture to</param>
        /// <param name="ppo">Postdata</param>
        /// <returns></returns>
        private XmlDocument UploadPicture(string url, PicturePostObject ppo, Twitter.Account account)
        {
            try
            {
                HttpWebRequest request = WebRequestFactory.CreateHttpRequest(url);
                request.Timeout = 20000;
                request.AllowWriteStreamBuffering = false; // don't want to buffer 3MB files, thanks
                request.AllowAutoRedirect = false;

                Multipart contents = new Multipart();
                contents.UploadPart += new Multipart.UploadPartEvent(contents_UploadPart);
                contents.Add("api_key", API_KEY);
                contents.Add("source", API_ORIGIN_ID);
                if (!string.IsNullOrEmpty(ppo.Message))
                    contents.Add("message", ppo.Message);
                else
                    contents.Add("message", "");

                string contentType = "";
                foreach (MediaType ft in this.API_FILETYPES)
                {
                    if (ft.Extension.Equals(Path.GetExtension(ppo.Filename).Substring(1), StringComparison.InvariantCultureIgnoreCase))
                    {
                        contentType = ft.ContentType;
                        break;
                    }
                }

                contents.Add("media", ppo.PictureStream, Path.GetFileName(ppo.Filename), contentType);

                OAuthAuthorizer.AuthorizeEcho(request, account.OAuth_token, account.OAuth_token_secret);

                return contents.UploadXML(request);
            }
            catch (Exception)
            {
                //Socket exception 10054 could occur when sending large files.
                // Should be more specific

                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                return null;
            }
        }

        private void contents_UploadPart(object sender, long bytesSent, long bytesTotal)
        {
            OnUploadPart(new PictureServiceEventArgs((int)bytesSent, (int)bytesSent, (int)bytesTotal));
        }
        #endregion
    }
}
