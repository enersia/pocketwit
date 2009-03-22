﻿using System;

using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Yedda
{
    public abstract class PictureServiceBase: IPictureService
    {
        #region private variables

        protected string PT_DEFAULT_FILENAME = string.Empty;
        protected int PT_READ_BUFFER_SIZE = 512;
        protected bool PT_USE_DEFAULT_FILENAME = true;
        protected bool PT_USE_DEFAULT_PATH = true;
        protected string PT_DEFAULT_PATH = string.Empty;
        protected string PT_ROOT_PATH = string.Empty;

        protected string API_SAVE_TO_PATH { get; set; }
        protected string API_SERVICE_NAME = string.Empty;

        #endregion


        #region IPictureService Members

        public abstract void PostPicture(PicturePostObject postData);

        public abstract void FetchPicture(string pictureURL);

        #region getters and setters

        public string SERVICE_NAME 
        {
            get
            {
                return API_SERVICE_NAME;
            }
        }

        /// <summary>
        /// Must be implemented to set fetching yes/no
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        public abstract bool CanFetchUrl(string URL);

        public bool HasEventHandlersSet { get; set; }

        public bool UseDefaultFileName
        {
            set
            {
                PT_USE_DEFAULT_FILENAME = value;
            }
            get
            {
                return PT_USE_DEFAULT_FILENAME;
            }
        }

        public string DefaultFileName
        {
            set
            {
                PT_DEFAULT_FILENAME = value;
            }
            get
            {
                return PT_DEFAULT_FILENAME;
            }
        }

        public bool UseDefaultFilePath
        {
            set
            {
                PT_USE_DEFAULT_PATH = value;
            }
            get
            {
                return PT_USE_DEFAULT_PATH;
            }
        }

        public string DefaultFilePath
        {
            set
            {
                PT_DEFAULT_PATH = value;
            }
            get
            {
                return PT_DEFAULT_PATH;
            }
        }

        public string RootPath
        {
            set
            {
                PT_ROOT_PATH = value;
            }
            get
            {
                return PT_ROOT_PATH;
            }
        }

        public int ReadBufferSize
        {
            set
            {
                PT_READ_BUFFER_SIZE = value;
            }
            get
            {
                return PT_READ_BUFFER_SIZE;
            }
        }

        public string ServiceName
        {
            get
            {
                return SERVICE_NAME;
            }
        }

        #endregion

        #endregion

        #region private methods

        /// <summary>
        /// Lookup the path and filename intended for the image. When it does not exist, create it.
        /// </summary>
        /// <param name="imageId">Image ID</param>
        /// <returns>Path to save the picture in.</returns>
        protected string GetPicturePath(string pictureURL)
        {
            #region argument check

            if (string.IsNullOrEmpty(pictureURL))
            {

            }

            #endregion

            String picturePath = String.Empty;

            int imageIdStartIndex = pictureURL.LastIndexOf('?') + 1;
            string imageId = pictureURL.Substring(imageIdStartIndex, pictureURL.Length - imageIdStartIndex);

            string rootpath = string.Empty;
            if (PT_USE_DEFAULT_PATH)
            {
                rootpath = Path.Combine(PT_ROOT_PATH, PT_DEFAULT_PATH);
            }
            else
            {
                rootpath = Path.Combine(PT_ROOT_PATH, API_SAVE_TO_PATH);
            }
            if (!Directory.Exists(rootpath))
            {
                Directory.CreateDirectory(rootpath);
            }

            if (PT_USE_DEFAULT_FILENAME)
            {
                picturePath = rootpath + "\\" + PT_DEFAULT_FILENAME;
                if (File.Exists(picturePath))
                {
                    File.Delete(picturePath);
                }
            }
            else
            {
                string firstChar = imageId.Substring(0, 1);
                picturePath = Path.Combine(rootpath, firstChar);
                if (!System.IO.Directory.Exists(picturePath))
                {
                    System.IO.Directory.CreateDirectory(picturePath);
                }
                picturePath = picturePath + "\\" + imageId + ".jpg";
            }
            return picturePath;
        }

        /// <summary>
        /// Save the picture data to disk.
        /// </summary>
        /// <param name="picturePath"></param>
        /// <param name="pictureData"></param>
        /// <param name="bufferSize"></param>
        /// <returns></returns>
        protected bool SavePicture(String picturePath, byte[] pictureData, int bufferSize)
        {
            #region argument check
            if (String.IsNullOrEmpty(picturePath))
            {
                return false;
            }

            if (pictureData == null)
            {
                return false;
            }
            if (pictureData.Length == 0)
            {
                return false;
            }
            #endregion

            if (!File.Exists(picturePath))
            {
                using (FileStream pictureFile = File.Create(picturePath))
                {
                    pictureFile.Write(pictureData, 0, bufferSize);
                    pictureFile.Close();
                }
            }
            else
            {
                using (FileStream pictureFile = File.Open(picturePath, FileMode.Append, FileAccess.Write))
                {
                    pictureFile.Write(pictureData, 0, bufferSize);
                    pictureFile.Close();
                }
            }
            return true; ;
        }

        #endregion

        #region events

        public event UploadFinishEventHandler UploadFinish;
        public event DownloadFinishEventHandler DownloadFinish;
        public event ErrorOccuredEventHandler ErrorOccured;
        public event MessageReadyEventHandler MessageReady;

        protected virtual void OnDownloadFinish(PictureServiceEventArgs e)
        {
            if (DownloadFinish != null)
            {
                try
                {
                    DownloadFinish(this, e);
                }
                catch (Exception ex)
                {
                    //Always continue after a missed event
                }
            }
        }

        protected virtual void OnUploadFinish(PictureServiceEventArgs e)
        {
            if (UploadFinish != null)
            {
                try
                {
                    UploadFinish(this, e);
                }
                catch (Exception ex)
                {
                    //Always continue after a missed event
                }
            }
        }

        protected virtual void OnErrorOccured(PictureServiceEventArgs e)
        {
            if (ErrorOccured != null)
            {
                try
                {
                    ErrorOccured(this, e);
                }
                catch (Exception ex)
                {
                    //Always continue after a missed event
                }
            }
        }

        protected virtual void OnMessageReady(PictureServiceEventArgs e)
        {
            if (MessageReady != null)
            {
                try
                {
                    MessageReady(this, e);
                }
                catch (Exception ex)
                {
                    //Always continue after a missed event
                }
            }
        }

        #endregion

        #region helper methods

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

        protected string CreateContentPartMedia(string header)
        {
            StringBuilder contents = new StringBuilder();

            contents.Append(header);
            contents.Append("\r\n");
            contents.Append(string.Format("Content-Disposition:form-data; name=\"i\";filename=\"image.jpg\"\r\n"));
            contents.Append("Content-Type: image/jpeg\r\n");
            contents.Append("\r\n");

            return contents.ToString();
        }

        protected string CreateContentPartPicture(string header)
        {
            StringBuilder contents = new StringBuilder();

            contents.Append(header);
            contents.Append("\r\n");
            contents.Append(string.Format("Content-Disposition:form-data; name=\"media\";filename=\"image.jpg\"\r\n"));
            contents.Append("Content-Type: image/jpeg\r\n");
            contents.Append("\r\n");

            return contents.ToString();
        }

        #endregion
    }
}
