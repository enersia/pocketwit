using System;

using System.Collections.Generic;
using System.Text;
using Yedda;
using System.Xml;
using System.IO;
using System.Net;

namespace PockeTwit.MediaServices
{

    public class TwitGoo : PictureServiceBase
    {
        #region private properties

        private static volatile TwitGoo _instance;
        private static object syncRoot = new Object();
        
        private const string API_UPLOAD = "http://twitgoo.com/api/upload";
        private const string API_UPLOAD_POST = "http://twitgoo.com/api/uploadAndPost";
        private const string API_SHOW_THUMB = "http://twitgoo.com/api/message/info/";  //The extra / for directly sticking the image-id on.


        private const string API_ERROR_UPLOAD = "Failed to upload picture to TwitGoo.";
        private const string API_ERROR_NOTREADY = "A request is already running.";
        private const string API_ERROR_DOWNLOAD = "Unable to download picture, try again later.";

        #endregion

        #region private objects

        private System.Threading.Thread workerThread;
        private PicturePostObject workerPPO;

        #endregion

        #region constructor
        /// <summary>
        /// Private constructor for usage in singleton.
        /// </summary>
        private TwitGoo()
        {
            API_SAVE_TO_PATH = "\\ArtCache\\www.twitgoo.com\\";
            API_SERVICE_NAME = "TwitGoo";
            API_CAN_UPLOAD_GPS = false;
            API_CAN_UPLOAD_MESSAGE = false;
            API_URLLENGTH = 30;

            API_FILETYPES.Add(new MediaType("jpg", "image/jpg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("jpeg", "image/jpg", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("gif", "image/gif", MediaTypeGroup.PICTURE));
            API_FILETYPES.Add(new MediaType("png", "image/png", MediaTypeGroup.PICTURE));
        }

        /// <summary>
        /// Singleton constructor
        /// </summary>
        /// <returns></returns>
        public static TwitGoo Instance
        {
           get
           {
               if (_instance == null)
               {
                   lock (syncRoot)
                   {
                       if (_instance == null)
                       {
                           _instance = new TwitGoo();
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
            #region Argument check

            //Check for empty path
            if (string.IsNullOrEmpty(postData.Filename))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "",API_ERROR_UPLOAD));
                return false;
            }

            //Check for empty credentials
            if (string.IsNullOrEmpty(account.OAuth_token_secret) ||
                string.IsNullOrEmpty(account.OAuth_token))
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, "", API_ERROR_UPLOAD));
                return false;
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

                    if (uploadResult.SelectSingleNode("rsp").Attributes["status"].Value == "fail")
                    {
                        string ErrorText = uploadResult.SelectSingleNode("//err").Attributes["msg"].Value;
                        OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, ErrorText));
                        return false;
                    }
                    else
                    {
                        string URL = uploadResult.SelectSingleNode("//mediaurl").InnerText;
                        postData.URL = URL;
                        OnUploadFinish(new PictureServiceEventArgs(PictureServiceErrorLevel.OK, URL, string.Empty, postData.Filename));
                        return true;
                    }
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
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
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
                    OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.NotReady, string.Empty, "A request is already running."));
                }
            }
            catch (Exception)
            {
                OnErrorOccured(new PictureServiceEventArgs(PictureServiceErrorLevel.Failed, string.Empty, API_ERROR_DOWNLOAD));
            } 
        }

        /// <summary>
        /// Test whether the service can fetch the URL.
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public override bool CanFetchUrl(string URL)
        {
            //need to build a request for it to get the correct url, the www part of the urls changes between pics.
            
            const string siteMarker = "twitgoo";
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
        private string RetrievePicture(string imageId)
        {
            string responseString = string.Empty;
            try
            {
                //We use the "iphone" optimized images
                HttpWebRequest myRequest = WebRequestFactory.CreateHttpRequest(API_SHOW_THUMB + imageId);
                myRequest.Method = "GET";
                

                using (WebResponse responseA = myRequest.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(responseA.GetResponseStream()))
                    {
                        XmlDocument responseXML = new XmlDocument();
                        string resp = reader.ReadToEnd();
                        responseXML.LoadXml(resp);

                        if (responseXML == null)
                        {
                            return string.Empty;
                        }
                        //imageurl for a bigger picture
                        responseString = responseXML.SelectSingleNode("rsp//thumburl").InnerText;
                        if (string.IsNullOrEmpty(responseString))
                        {
                            return string.Empty;
                        }

                        reader.Close();
                    }
                    responseA.Close();
                }

                

                //We use the "iphone" optimized images
                myRequest = WebRequestFactory.CreateHttpRequest(responseString);
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

                request.PreAuthenticate = true;
                request.AllowWriteStreamBuffering = false;
                request.Timeout = 60000;

                Multipart contents = new Multipart();
                contents.UploadPart += new Multipart.UploadPartEvent(contents_UploadPart);

                contents.Add("media", ppo.PictureStream, Path.GetFileName(ppo.Filename));

                OAuth.OAuthAuthorizer.AuthorizeEcho(request, account.OAuth_token, account.OAuth_token_secret);
                return contents.UploadXML(request);
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

        #region helper functions

        protected string CreateContentPartString(string header, string dispositionName, string valueToSend)
        {
            StringBuilder contents = new StringBuilder();

            contents.Append(header);
            contents.Append("\r\n");
            contents.Append(String.Format("Content-Disposition: form-data;name=\"{0}\"\r\n", dispositionName));
            contents.Append("\r\n");
            contents.Append(valueToSend);
            contents.Append("\r\n");

            return contents.ToString();
        }

        protected string CreateContentPartPicture(string header, string filename)
        {
            StringBuilder contents = new StringBuilder();

            contents.Append(header);
            contents.Append("\r\n");
            contents.Append(string.Format("Content-Disposition:form-data; name=\"media\";filename=\"{0}\"\r\n", filename));
            contents.Append("Content-Type: image/jpeg\r\n");
            contents.Append("\r\n");

            return contents.ToString();
        }

        #endregion

    }
}
