using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;

namespace PockeTwit.MediaServices
{
    public class AttachmentServiceEventArgs : EventArgs
    {
        private UploadAttachment _attachment;
        private long _bytesTotal;
        private long _bytesProgress;

        public AttachmentServiceEventArgs(UploadAttachment attachment): this(attachment, 0, 0)
        {
            
        }

        public AttachmentServiceEventArgs(UploadAttachment attachment, long bytesTotal, long bytesProgress)
        {
            _attachment = attachment;
            _bytesTotal = bytesTotal;
            _bytesProgress = bytesProgress;
        }

        public long BytesTotal { get { return _bytesTotal; } }
        public long BytesProgress { get { return _bytesProgress; } }
        public UploadAttachment Attachment { get { return _attachment; } }
    }

    public delegate void AttachmentEventHandler(object sender, AttachmentServiceEventArgs eventArgs);

    [Flags]
    public enum UploadCapabilities
    {
        Message = 0x01,
        Position = 0x02
    }

    public class AttachmentUploadException : InvalidOperationException
    {
        /*        public enum UploadErrorClass
                {
                    ClientError, // an HTTP 4xx error (don't try again without changes)
                    ServerError, // an HTTP 5xx error (try again later)
                    ConnectionError, // bad DNS, no route to host etc (try again later)
                    TransportError // bad network conditions, e.g. connection dropped (try again immediately)
            
                    // Note that a timeout is one of connection or transport error, depending when it occurs
                }
                public enum UploadErrorDetail
                {
                    // Expected HTTP Errors
                    // 4xx
                    BadRequest   = HttpStatusCode.BadRequest, // 400
                    Unauthorized = HttpStatusCode.Unauthorized,  // 401
                    Forbidden = HttpStatusCode.Forbidden,  // 403
                    NotFound = HttpStatusCode.NotFound,  // 404
                    MethodNotAllowed = HttpStatusCode.MethodNotAllowed,  // 405
                    UnsupportedMediaType = HttpStatusCode.UnsupportedMediaType,  // 415

                    // 5xx
                    InternalServerError = HttpStatusCode.InternalServerError, // 500
                    NotImplemented = HttpStatusCode.NotImplemented, // 501
                    BadGateway = HttpStatusCode.NotImplemented, // 501

                }
                public UploadErrorClass ErrorClass { get; set; }
                public UploadErrorClass ErrorDetail { get; set; }*/
        public AttachmentUploadException(/*UploadErrorClass ErrorClass,*/ string message, Exception innerException)
            : base(message, innerException)
        {
            //this.ErrorClass = ErrorClass;
        }
    }

    public interface IUploadService
    {
        string ServiceName { get; }
        /*        UploadCapabilities Capabilities { get; }
                int UriLength { get; }
                List<MediaType> FileTypes { get; }*/
        
        // Uploads the attachment. Must set the Uri on success
        Uri UploadAttachment(UploadAttachment Attachment, Yedda.Twitter.Account account);

/*        event AttachmentEventHandler UploadFinish;
        event AttachmentEventHandler ErrorOccured;
        event AttachmentEventHandler UploadPart;*/
    }

    // as opposed to a DownloadAttachment
    public class UploadAttachment
    {
        public enum AttachmentStatus
        {
            Pending = 0,    // waiting for upload to start
            Uploading = 10,
            Complete = 20,
            Error = 30       
        }
        public UploadAttachment(string FileName)
            : this(FileName, null, null)
        {
            
        }

        public UploadAttachment(string FileName, string Caption)
            : this(FileName, Caption, null)

        {
            
        }

        public UploadAttachment(string FileName, string Caption, PockeTwit.Position.GeoCoord Position)
        {
            this.SourceFileName = FileName;
            this.Name = this.SourceFileName; //Path.GetFileName(FileName);
            this.UploadedUri = null;
            this.Status = AttachmentStatus.Pending;
            this.Caption = Caption;
            this.Position = Position;
        }
        private string SourceFileName; // the name

        public Stream GetStream()
        {
            return new FileStream(SourceFileName, FileMode.Open);
        } // the data
        public string Name  { get; set; } // the name
        public Uri UploadedUri { get; set; } // where to find it, or empty if not uploaded
        public AttachmentStatus Status { get; set; }
        public string Caption { get; set; }
        public MediaType MediaType { get; set; }
        public PockeTwit.Position.GeoCoord Position { get; set; }
        // Temporary, until replaced by MediaService

    }

    public class AttachmentUploadManager : System.Collections.IEnumerable
    {
        public enum UploadStatus
        {
            None, // nothing uploaded
            InProgress, // an upload is in progress
            Partial, // partially uploaded
            Complete // all uploaded successfully
        }

        public AttachmentUploadManager() : this(null)
        {
        }

        public AttachmentUploadManager(IPictureService PictureService)
        {
            if (PictureService != null)
            {
                foreach (MediaType type in PictureService.FileTypes)
                {
                    if (type.MediaGroup == MediaTypeGroup.PICTURE)
                        this.PictureService = PictureService;
                    else if (type.MediaGroup == MediaTypeGroup.VIDEO)
                        this.VideoService = PictureService;
                    else if (type.MediaGroup == MediaTypeGroup.AUDIO)
                        this.AudioService = PictureService;
                    else if (type.MediaGroup == MediaTypeGroup.DOCUMENT)
                        this.DocumentService = PictureService;
                }
            }
            IPictureService Posterous = PictureServiceFactory.Instance.GetServiceByName("Posterous");
            if (this.PictureService == null)
                this.PictureService = Posterous;
            if (this.VideoService == null)
                this.VideoService = Posterous;
            if (this.AudioService == null)
                this.AudioService = Posterous;
            if (this.DocumentService == null)
                this.DocumentService = Posterous;
        }
        // TO BE DEPRECATED!
        private IPictureService PictureService;
        private IPictureService VideoService;
        private IPictureService AudioService;
        private IPictureService DocumentService;

        public Yedda.Twitter.Account Account {get; set; }
        public UploadStatus Status
        {
            get
            {
                UploadStatus Stat = UploadStatus.Complete;
                bool None = true;
                foreach (UploadAttachment a in Attachments)
                {
                    if (a.Status == UploadAttachment.AttachmentStatus.Uploading)
                    {
                        Stat = UploadStatus.InProgress;
                        break;
                    }
                    else if (a.Status == UploadAttachment.AttachmentStatus.Complete)
                    {
                        None = false;
                    }
                    else
                        Stat = UploadStatus.Partial;
                }
                if (None)
                    Stat = UploadStatus.None;
                return Stat;
            }
        }
#region List Management
        private List<UploadAttachment> Attachments = new List<UploadAttachment>();
        public void AddAttachment(UploadAttachment Attachment)
        {
            if (Attachments.IndexOf(Attachment) < 0)
            {

                Attachment.MediaType = GetMatchingType(Attachment);
                Attachments.Add(Attachment);
            }
        }

        public void DeleteAttachment(UploadAttachment Attachment)
        {
            Attachments.Remove(Attachment);
        }

        public void Clear()
        {
            Attachments.Clear();
        }

        public UploadAttachment[] GetAttachments()
        {
            UploadAttachment[] res = new UploadAttachment[Attachments.Count];
            Attachments.CopyTo(res);
            return res;

        }

        public int Count { get { return Attachments.Count; } }

#endregion

        public int GetUriSpaceRequired()
        {
            return GetUriSpaceRequired(string.Empty);
        }

        public int GetUriSpaceRequired(string MessageText)
        {
            // WRONG!
            
            
            int Length = 0; // spaces to separate URIs
            foreach (UploadAttachment Attachment in Attachments)
            {
                if (Attachment.UploadedUri != null)
                {

                    string Uri = Attachment.UploadedUri.OriginalString;
                    if(!MessageText.Contains(Uri))
                        Length += (Uri.Length + 1);
                    // otherwise already included, so don't need to add it
                }
                else
                {
                    // this will be really slow and should be sorted out
                    MediaType t = GetMatchingType(Attachment);
                    if (t == null) continue;
                    IPictureService UploadService = SelectService(t.MediaGroup);
                    if (UploadService == null) continue;

                    Length += UploadService.UrlLength;
                    // need to use the relevant service to get the max URI
                }
            }
            return Length;
        }

        public string AppendLinks(string MessageText)
        {
            string ToAppend = "";

            foreach (UploadAttachment Attachment in Attachments)
            {
                if (Attachment.UploadedUri != null)
                {
                    string Uri = Attachment.UploadedUri.OriginalString;
                    if (!MessageText.Contains(Uri))
                        ToAppend += " " + Uri;
                    // otherwise already included, so don't need to add it
                }
                else
                {
                    // it's buggered
                }
            }
            return MessageText + ToAppend;
        }

        public bool Upload(int i)
        {
            return Upload(Attachments[i]);
        }

        public bool Upload(UploadAttachment a)
        {
            if (Attachments.Contains(a))
                return DoUpload(a);
            else
                throw new InvalidOperationException("Attachment not attached");
        }

        public System.Collections.IEnumerator GetEnumerator()
        {
            return Attachments.GetEnumerator();
        }

        public UploadAttachment this[int i]
        {
            get { return Attachments[i]; }
        }

        // Solely designed to convert from AttachmentUploadManager to
        // IPictureService until we get IMediaServices up and running
        // Let's do it one step at a time!
        private bool DoUpload(UploadAttachment a)
        {
            MediaType type = GetMatchingType(a);
            if (type == null)
            {
                a.Status = UploadAttachment.AttachmentStatus.Error;
                return false;
            }
            IPictureService Service = SelectService(type.MediaGroup);
            if (Service == null)
            {
                a.Status = UploadAttachment.AttachmentStatus.Error;
                return false;
            }

            IUploadService UService = Service as IUploadService;
            if (UService != null)
                return DoUpload(UService, a);
            else
                return DoUpload(Service, a);
        }

        private bool DoUpload(IPictureService Service, UploadAttachment a)
        {
            PicturePostObject ppo = new PicturePostObject();
            if(a.MediaType != null)
                ppo.ContentType = a.MediaType.ContentType;
            if (a.Position != null)
            {
                ppo.Lat = a.Position.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
                ppo.Lon = a.Position.Lat.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            ppo.Message = a.Caption;
            //ppo.PictureStream = a.Contents;
            ppo.Filename = a.Name;
            ppo.UseAsync = false;

            // This is only temporary -> PostUpdate form has event handlers reg'd
            a.Status = UploadAttachment.AttachmentStatus.Uploading;
            Service.PostPicture(ppo, Account);
            if (!string.IsNullOrEmpty(ppo.URL))
            {
                a.UploadedUri = new Uri(ppo.URL);
                a.Status = UploadAttachment.AttachmentStatus.Complete;
            }
            else
            {
                a.Status = UploadAttachment.AttachmentStatus.Error;
            }
            return true;
        }

        private bool DoUpload(IUploadService Service, UploadAttachment a)
        {
            try
            {
                a.Status = UploadAttachment.AttachmentStatus.Uploading;
                a.UploadedUri = Service.UploadAttachment(a, Account);
                a.Status = UploadAttachment.AttachmentStatus.Complete;
                return true;
            }
            catch (WebException we)
            {
                if (we.Response is HttpWebResponse)
                {
                    using (HttpWebResponse response = we.Response as HttpWebResponse)
                    {
                        Localization.LocalizedMessageBox.Show(
                                string.Format(Localization.XmlBasedResourceManager.GetString("The media service encountered an error: {0} {1}"), response.StatusCode, response.StatusDescription)
                            );
                    }
                }
                else
                    Localization.LocalizedMessageBox.Show("An unknown error occurred while uploading.");
            }
            catch (System.Net.Sockets.SocketException se)
            {
                string LocalizedString = null;
                switch (se.NativeErrorCode)
                {
                    // Can't contact server
                    case 10050: // Netdown
                    case 10051: // Unreachable
                    case 10061: // Refused
                    case 10062: // Cannot translate name
                    case 10064: // Host down
                    case 10065: // No route to host
                    case 11001: // Hostname not found
                    case 11002: // Host temporarily not found
                    case 11003: // Non-recoverable host lookup error
                    case 11004: // No data record for name - IP address lookup failure
                        LocalizedString = string.Format(Localization.XmlBasedResourceManager.GetString("The media service can not be reached (Error {0})."), se.NativeErrorCode);
                        break;

                    // Interrupted connection
                    case 10004: // Airplane mode
                    case 10052: // Network Reset
                    case 10053: // Connection Aborted
                    case 10054: // Connection Reset
                        LocalizedString = Localization.XmlBasedResourceManager.GetString("Connection to service lost.");
                        break;

                    // Timeout
                    case 10060: // Timeout
                        LocalizedString = Localization.XmlBasedResourceManager.GetString("Connection timed out.");
                        break;

                    default:
                        LocalizedString = string.Format(Localization.XmlBasedResourceManager.GetString("The media service encountered an error: {0} {1}"), se.NativeErrorCode, se.Message);
                        break;
                }
                Localization.LocalizedMessageBox.Show(
                        LocalizedString
                    );
            }
            catch (InvalidOperationException)
            {
                Localization.LocalizedMessageBox.Show("An internal error occurred while uploading.");
            }
            catch (Exception)
            {
                Localization.LocalizedMessageBox.Show("An unknown error occurred while uploading.");
            }
            a.Status = UploadAttachment.AttachmentStatus.Error;
            return false;
        }

        public bool CanUpload(MediaTypeGroup mediaGroup)
        {
            return (SelectService(mediaGroup) != null);
        }

        private MediaType GetMatchingType(UploadAttachment a)
        {
            string ext = Path.GetExtension(a.Name);
            if (!string.IsNullOrEmpty(ext))
                ext = ext.Substring(1);
            else
                return null;

            IPictureService srv = null;

            MediaTypeGroup[] groups = {
                                             MediaTypeGroup.PICTURE,
                                             MediaTypeGroup.VIDEO,
                                             MediaTypeGroup.AUDIO,
                                             MediaTypeGroup.DOCUMENT
                                   };

            foreach (MediaTypeGroup group in groups)
            {
                srv = SelectService(group);
                if (srv != null)
                   foreach (MediaType r in srv.FileTypes)
                        if (r.MediaGroup == group && r.Extension.Equals(ext, StringComparison.InvariantCultureIgnoreCase))
                            return r;
            }
            return null;
        }

        private IPictureService SelectService(MediaTypeGroup mediaGroup)
        {
            // will select a service to use
            if (PictureService == null) return null;

            switch (mediaGroup)
            {
                case MediaTypeGroup.PICTURE:
                    return PictureService;

                case MediaTypeGroup.VIDEO:
                    return VideoService;

                case MediaTypeGroup.AUDIO:
                    return AudioService;

                case MediaTypeGroup.DOCUMENT:
                    return DocumentService;
                    
                case MediaTypeGroup.ALL: // not a valid thing to ask for
                default:
                    return null; 
            }
        }


        public string GetFileFilter(MediaTypeGroup mediaGroup)
        {
            bool first = true;
            string filterFormat = "{0} ({1})|{1}";
            string unnamedFormat = Localization.XmlBasedResourceManager.GetString("{0} files");
            string extFormat = "*.{0}";
            StringBuilder sb = new StringBuilder();
            if (mediaGroup == MediaTypeGroup.ALL)
            {
                MediaTypeGroup[] groups = {
                                             MediaTypeGroup.PICTURE,
                                             MediaTypeGroup.VIDEO,
                                             MediaTypeGroup.AUDIO,
                                             MediaTypeGroup.DOCUMENT
                                   };
                Dictionary<MediaTypeGroup, string> descriptions = new Dictionary<MediaTypeGroup,string>(4);
                descriptions.Add(MediaTypeGroup.PICTURE, Localization.XmlBasedResourceManager.GetString("Pictures"));
                descriptions.Add(MediaTypeGroup.VIDEO, Localization.XmlBasedResourceManager.GetString("Videos"));
                descriptions.Add(MediaTypeGroup.AUDIO, Localization.XmlBasedResourceManager.GetString("Audio Files"));
                descriptions.Add(MediaTypeGroup.DOCUMENT, Localization.XmlBasedResourceManager.GetString("Other Documents"));

                // Aggregate file types for each MediaTypeGroup
                foreach (MediaTypeGroup group in groups)
                {
                    StringBuilder groupExtensions = new StringBuilder();
                    IPictureService srv = SelectService(group);
                    first = true;
                    if (srv != null)
                        foreach (MediaType r in srv.FileTypes)
                            if (r.MediaGroup == group)
                            {
                                if (first)
                                    first = false;
                                else
                                    groupExtensions.Append(",");
                                groupExtensions.Append(string.Format(extFormat, r.Extension));
                            }
                    if (groupExtensions.Length > 0)
                    {
                        if (sb.Length > 0)
                            sb.Append("|");
                        sb.Append(string.Format(filterFormat, descriptions[group], groupExtensions.ToString()));
                    }
                }
                // Now get individual file types
                foreach (MediaTypeGroup group in groups)
                {
                    string filter = GetFileFilter(group);
                    if (sb.Length > 0 && !string.IsNullOrEmpty(filter))
                        sb.Append("|");
                    sb.Append(filter);
                }
                return sb.ToString();
            }
            else
            {
                IPictureService Service = SelectService(mediaGroup);
                if (Service == null) return string.Empty;
                foreach (MediaType type in Service.FileTypes)
                {
                    if (type.MediaGroup != mediaGroup)
                    {
                        continue;
                    }

                    if (first)
                        first = false;
                    else
                        sb.Append("|");

                    sb.Append(string.Format(filterFormat, string.Format(unnamedFormat, type.Extension), string.Format(extFormat, type.Extension)));

                }
                return sb.ToString();
            }
        }
    }


}
