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
    public class yFrog : PictureServiceBase
    {
        #region private properties

        private static volatile yFrog _instance;
        private static object syncRoot = new Object();
        
        private const string API_UPLOAD = "http://yFrog.com/api/xauth_upload";
        private const string API_UPLOAD_POST = "http://yFrog.com/api/xauth_upload";
        private const string API_SHOW_THUMB = "http://yFrog.com/";  //The extra / for directly sticking the image-id on.

        private const string API_ERROR_UPLOAD = "Failed to upload picture to yFrog.";
        private const string API_ERROR_NOTREADY = "A request is already running.";
        private const string API_ERROR_DOWNLOAD = "Unable to download picture, try again later.";

        #endregion

        #region private objects

        private System.Threading.Thread workerThread;
        private PicturePostObject workerPPO;

        private Twitter.Account account = null;

        #endregion

        #region constructor
        /// <summary>
        /// Private constructor for usage in singleton.
        /// </summary>
        private yFrog()
        {
            API_SAVE_TO_PATH = "\\ArtCache\\www.yFrog.com\\";
            API_SERVICE_NAME = "yFrog";
            API_CAN_UPLOAD_GPS = true;
            API_CAN_UPLOAD_MESSAGE = false;
            API_URLLENGTH = 29;

            API_FILETYPES.Add(new MediaType("jpg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("jpeg", "image/jpeg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("gif", "image/gif", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("png", "image/png", MediaTypeGroup.PICTURE));

            API_FILETYPES.Add(new MediaType("bmp", "image/bmp", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("tiff", "image/tiff", MediaTypeGroup.PICTURE));

            API_FILETYPES.Add(new MediaType("flv", "video/x-flv", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mpeg", "video/mpeg", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mpg", "video/mpeg", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("wmv", "video/x-ms-wmv", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mkv", "video/x-matroska", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mov", "video/quicktime", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("mp4", "video/mp4", MediaTypeGroup.VIDEO));

            API_FILETYPES.Add(new MediaType("avi", "video/avi", MediaTypeGroup.VIDEO));
            API_FILETYPES.Add(new MediaType("3gp", "video/3gp", MediaTypeGroup.VIDEO));

        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static yFrog Instance
        {
           get
           {
               if (_instance == null)
               {
                   lock (syncRoot)
                   {
                       if (_instance == null)
                       {
                           _instance = new yFrog();
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
        /// Post a picture.
        /// </summary>
        /// <param name="postData"></param>
        public override bool PostPicture(PicturePostObject postData, Twitter.Account account)
        {
            return DoPost(postData, account, true);
        }

        /// <summary>
        /// Post a picture.
        /// </summary>
        /// <param name="postData"></param>
        public bool DoPost(PicturePostObject postData, Twitter.Account account, bool successEvent)
        {
            #region Argument check

            //Check for empty path
            if (string.IsNullOrEmpty(postData.Filename))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "Failed to upload picture to yFrog.", ""));
            }

            //Check for empty credentials
            if (string.IsNullOrEmpty(account.OAuth_token) ||
                string.IsNullOrEmpty(account.OAuth_token_secret))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "Failed to upload picture to yFrog.", ""));
            }

            #endregion

            using (System.IO.FileStream file = new FileStream(postData.Filename, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    postData.PictureStream = file;
                    XmlDocument uploadResult = UploadPicture(API_UPLOAD, postData, account);

                    if (uploadResult == null)
                    {
                        return false;
                    }

                    if (uploadResult.SelectSingleNode("rsp").Attributes["stat"].Value == "fail")
                    {
                        string ErrorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                        OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, ErrorText, ""));
                    }
                    else if (successEvent)
                    {
                        string URL = uploadResult.SelectSingleNode("//mediaurl").InnerText;
                        postData.URL = URL;
                        OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, postData.Filename));
                    }
                    return true;
                }
                catch (Exception)
                {
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_UPLOAD));
                    return false;
                }
            }
        }


        /// <summary>
        /// Fetch a URL.
        /// </summary>
        /// <param name="pictureURL"></param>
        public override void FetchPicture(string pictureURL, Twitter.Account account)
        {
            #region Argument check

            //Need a url to read from.
            if (string.IsNullOrEmpty(pictureURL))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", "Failed to download picture from yFrog."));
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
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", "Failed to download picture from yFrog."));
            } 
        }

        /// <summary>
        /// Test whether the service can fetch the URL.
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public override bool CanFetchUrl(string URL)
        {
            const string siteMarker = "yfrog";
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
                    OnDownloadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, resultFileName, "", pictureURL));
                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_DOWNLOAD));
            }
            workerThread = null;
        }

        private void ProcessUpload()
        {
            try
            {
                XmlDocument uploadResult = UploadPicture(API_UPLOAD, workerPPO, this.account);
                if (uploadResult == null)
                {
                    workerThread = null;
                    return;
                }

                if (uploadResult.SelectSingleNode("rsp").Attributes["stat"].Value == "fail")
                {
                    string ErrorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, ErrorText, ""));
                }
                else
                {
                    string URL = uploadResult.SelectSingleNode("//mediaurl").InnerText;
                    //postData.URL = URL;
                    OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK,URL,"",workerPPO.Filename));
                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_DOWNLOAD));
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
                //We use the "iphone" optimized images
                HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(API_SHOW_THUMB + imageId + ":iphone");
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
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_DOWNLOAD));
                return string.Empty;
            }
        }

        /// <summary>
        /// Upload a picture
        /// </summary>
        /// <param name="url"></param>
        /// <param name="ppo"></param>
        /// <returns></returns>
        private XmlDocument UploadPicture(string url, PicturePostObject ppo, Twitter.Account account)
        {
            try
            {
                HttpWebRequest request = WebRequestFactory.CreateHttpRequest(url);

                string boundary = System.Guid.NewGuid().ToString();
                request.Timeout = 20000;
                request.AllowAutoRedirect = false;
                request.AllowWriteStreamBuffering = false;
                request.PreAuthenticate = true;

                Multipart content = new Multipart();
                content.UploadPart += new Multipart.UploadPartEvent(contents_UploadPart);

                content.Add("source", "pocketwit");

                if (!string.IsNullOrEmpty(ppo.Lat) && !string.IsNullOrEmpty(ppo.Lon))
                {
                    string geotag = string.Format("geotagged,geo:lat={0},geo:lon={1}", ppo.Lat, ppo.Lon);
                    content.Add("tags", geotag);
                }
                
                if (!string.IsNullOrEmpty(ppo.Message))
                {
                    content.Add("message", ppo.Message);
                }

                content.Add("media", ppo.PictureStream, Path.GetFileName(ppo.Filename), ppo.ContentType);

                OAuthAuthorizer.AuthorizeEcho(request, account.OAuth_token, account.OAuth_token_secret, Twitter.OutputFormatType.XML);

                return content.UploadXML(request);
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_UPLOAD));
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
