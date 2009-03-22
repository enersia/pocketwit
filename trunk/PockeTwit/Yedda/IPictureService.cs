﻿using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;

namespace Yedda
{
    
    public class PictureServiceEventArgs : EventArgs
    {
        private PictureServiceErrorLevel _pictureServiceErrorLevel = PictureServiceErrorLevel.OK; 
        private string _returnMessage = string.Empty;
        private string _errorMessage = string.Empty;
        private string _pictureFileName = string.Empty;
        private int _totalBytesDownloaded = -1;
        private int _totalBytesToDownload = -1;
        private int _bytesDownloaded = -1;
        
        // Constructors
        public PictureServiceEventArgs(PictureServiceErrorLevel pictureServiceErrorLevel, string returnMessage, string errorMessage)
        {
            _pictureServiceErrorLevel = pictureServiceErrorLevel;
            _returnMessage = returnMessage;
            _errorMessage = errorMessage;
        }
        public PictureServiceEventArgs(PictureServiceErrorLevel pictureServiceErrorLevel, string returnMessage, string errorMessage, string pictureFileName)
        {
            _pictureServiceErrorLevel = pictureServiceErrorLevel;
            _returnMessage = returnMessage;
            _errorMessage = errorMessage;
            _pictureFileName = pictureFileName;
        }
        public PictureServiceEventArgs(int bytesDownloaded, int totalBytesDownloaded, int totalBytesToDownload)
        {
            _bytesDownloaded = bytesDownloaded;
            _totalBytesDownloaded = totalBytesDownloaded;
            _totalBytesToDownload = totalBytesToDownload;
        }

        public PictureServiceErrorLevel ErrorLevel
        {
            get { return _pictureServiceErrorLevel; }
        }
        public string ReturnMessage
        {
            get { return _returnMessage; }
        }
        public string ErrorMessage
        {
            get { return _errorMessage; }
        }
        public string PictureFileName
        {
            get { return _pictureFileName; }
        }
        public int TotalBytesDownloaded
        {
            get { return _totalBytesDownloaded; }
        }
        public int TotalBytesToDownload
        {
            get { return _totalBytesToDownload; }
        }
        public int BytesDownloaded
        {
            get { return _bytesDownloaded; }
        }
    }

    public enum PictureServiceErrorLevel
    {
        OK = 0,
        NotReady = 10,
        UnAvailable = 20,
        Failed = 99

        //Allways room te expand.
    }

    public delegate void UploadFinishEventHandler(object sender, PictureServiceEventArgs eventArgs);
    public delegate void DownloadFinishEventHandler(object sender, PictureServiceEventArgs eventArgs);
    public delegate void ErrorOccuredEventHandler(object sender, PictureServiceEventArgs eventArgs);
    public delegate void MessageReadyEventHandler(object sender, PictureServiceEventArgs eventArgs);
    public delegate void DownloadPartEventHandler(object sender, PictureServiceEventArgs eventArgs);


    /// <summary>
    /// Interface for multiple picture services
    /// </summary>
    public interface IPictureService
    {
        event UploadFinishEventHandler UploadFinish;
        event DownloadFinishEventHandler DownloadFinish;
        event ErrorOccuredEventHandler ErrorOccured;
        event MessageReadyEventHandler MessageReady;
        event DownloadPartEventHandler DownloadPart;

        /// <summary>
        /// Send a picture to a twitter picture framework
        /// </summary>
        /// <param name="postData">Postdata</param>
        /// <returns>Returned URL from server</returns>
        void PostPicture(PicturePostObject postData);

        /// <summary>
        /// Retrieve a picture from a picture service. 
        /// </summary>
        /// <param name="pictureURL">pictureURL</param>
        /// <returns>Local path for downloaded picture.</returns>
        void FetchPicture(string pictureURL);

        /// <summary>
        /// Check for possibility of getting the picture with current service.
        /// </summary>
        /// <param name="URL"></param>
        /// <returns></returns>
        bool CanFetchUrl(string URL);


        /// <summary>
        /// 
        /// </summary>
        bool HasEventHandlersSet { get; set; }
        bool UseDefaultFileName { set; get; }
        string DefaultFileName { set; get; }
        bool UseDefaultFilePath { set; get; }
        string DefaultFilePath { set; get; }
        string RootPath {  set; get; }
        int ReadBufferSize { set; get; }
        string ServiceName { get; }
    }

    /// <summary>
    /// State object for downloading in a-sync mode.
    /// </summary>
    public class AsyncStateData
    {
        internal int totalBytesToDownload = -1;
        internal int bytesRead = 0;
        internal int totalBytesRead = 0;

        internal byte[] dataHolder;
        internal string fileName;
        internal Stream dataStream;
        internal WebResponse response;
    }
}