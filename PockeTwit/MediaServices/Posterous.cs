using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;
using Yedda;
using OAuth;

namespace PockeTwit.MediaServices
{
    public class Posterous   : PictureServiceBase
    {
        #region private properties

        private static volatile Posterous _instance;
        private static object syncRoot = new Object();

        private const string API_UPLOAD = "http://posterous.com/api2/upload.xml";
        private const string API_UPLOAD_POST = "http://posterous.com/api2/upload.xml";
        private const string API_SHOW_THUMB = "http://posterous.com/show/thumb/";  //The extra / for directly sticking the image-id on.

        private const string API_ERROR_UPLOAD = "Unable to upload to Posterous";
        private const string API_ERROR_DOWNLOAD = "Unable to download from Posterous";

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
        private Posterous()
        {
            API_SAVE_TO_PATH = "\\ArtCache\\www.posterous.com\\";
            API_SERVICE_NAME = "Posterous";
            API_CAN_UPLOAD_GPS = false;
            API_CAN_UPLOAD_MESSAGE = false;
            API_CAN_UPLOAD = true;
            API_URLLENGTH = 28;

            API_FILETYPES.Add(new MediaType("jpg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("jpeg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("gif", "image/gif", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("png", "image/png", MediaTypeGroup.PICTURE));

            API_FILETYPES.Add(new MediaType("pdf", "text/pdf", MediaTypeGroup.DOCUMENT));
            API_FILETYPES.Add(new MediaType("txt", "text/plain", MediaTypeGroup.DOCUMENT));
            API_FILETYPES.Add(new MediaType("rtf", "application/rtf", MediaTypeGroup.DOCUMENT));
            API_FILETYPES.Add(new MediaType("docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", MediaTypeGroup.DOCUMENT));
            API_FILETYPES.Add(new MediaType("doc", "application/msword", MediaTypeGroup.DOCUMENT));
            API_FILETYPES.Add(new MediaType("ppt", "application/vnd.ms-powerpoint", MediaTypeGroup.DOCUMENT));
            API_FILETYPES.Add(new MediaType("pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation", MediaTypeGroup.DOCUMENT));
            API_FILETYPES.Add(new MediaType("ppsx", "application/vnd.openxmlformats-officedocument.presentationml.slideshow", MediaTypeGroup.DOCUMENT));
            API_FILETYPES.Add(new MediaType("xls", "application/vnd.ms-excel", MediaTypeGroup.DOCUMENT));
            API_FILETYPES.Add(new MediaType("xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", MediaTypeGroup.DOCUMENT));

            API_FILETYPES.Add(new MediaType("wmv", "video/x-ms-wmv", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mp4", "video/mp4", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("3gp", "video/3gpp", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("flv", "video/x-flv", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("asf", "video/x-ms-asf", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("avi", "video/avi", MediaTypeGroup.VIDEO));

            API_FILETYPES.Add(new MediaType("amr", "audio/amr", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("mp3", "audio/mpeg", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("wav", "audio/wav", MediaTypeGroup.AUDIO));
            API_FILETYPES.Add(new MediaType("wma", "audio/x-ms-wma", MediaTypeGroup.AUDIO));

        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static Posterous Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncRoot)
                    {
                        if (_instance == null)
                        {
                            _instance = new Posterous();
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
            #region Argument check

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

            _account = account;

            using (postData.PictureStream = new FileStream(postData.Filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    XmlDocument uploadResult = UploadPicture(API_UPLOAD, postData, account);

                    if (uploadResult == null)
                    {
                        return false;
                    }

                    string URL = uploadResult.SelectSingleNode("//url").InnerText;
                    OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, postData.Filename));
                    return true;
                }
                catch
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                    return false;
                }
            }
        }

        /// <summary>
        /// Fetch a picture
        /// </summary>
        /// <param name="pictureURL"></param>
        public override void FetchPicture(string pictureURL, Twitter.Account account)
        {
            #region Argument check

            //Need a url to read from.
            if (string.IsNullOrEmpty(pictureURL))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", "Failed to download picture from TwitPic."));
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
            catch 
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", "Failed to download picture from TwitPic."));
            }
        }



        /// <summary>
        /// Test whether this service can fetch a picture.
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public override bool CanFetchUrl(string URL)
        {
            const string siteMarker = "posterous";
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

                string resultFileName = RetrievePicture(imageID);

                if (!string.IsNullOrEmpty(resultFileName))
                {
                    OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, resultFileName, string.Empty, pictureURL));
                }
            }
            catch 
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
        private string RetrievePicture(string imageId)
        {
            try
            {
                HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(API_SHOW_THUMB + imageId);
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
            catch
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
                request.Timeout = 60000;
                request.AllowWriteStreamBuffering = false; // don't want to buffer 3MB files, thanks
                request.AllowAutoRedirect = false;

                Multipart contents = new Multipart();
                contents.UploadPart += new Multipart.UploadPartEvent(contents_UploadPart);
                contents.Add("source", "PockeTwit");
                contents.Add("sourceLink", "http://code.google.com/p/pocketwit/");
                if (!string.IsNullOrEmpty(ppo.Message))
                {
                    contents.Add("message", ppo.Message);
                }

                contents.Add("media", ppo.PictureStream, Path.GetFileName(ppo.Filename), ppo.ContentType);

                OAuthAuthorizer.AuthorizeEcho(request, account.OAuth_token, account.OAuth_token_secret);
                return contents.UploadXML(request);
            }
            catch (WebException ex)
            {
                if (ex.Response is HttpWebResponse)
                {
                    using(HttpWebResponse response = ex.Response as HttpWebResponse)
                    {
                        OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, String.Format(PockeTwit.Localization.XmlBasedResourceManager.GetString("Error Uploading: Service returned message '{0}'"), response.StatusDescription), ppo.Filename));
                    }
                }
                else if (ex.InnerException is System.Net.Sockets.SocketException)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, ex.Message, ppo.Filename));
                }
                else
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD, ppo.Filename));
                return null;
            }
            catch (Exception ex)
            {

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
