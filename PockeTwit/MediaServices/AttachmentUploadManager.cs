using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace PockeTwit.MediaServices
{
    public class AttachmentServiceEventArgs : EventArgs
    {
        private UploadAttachment _attachment;
        uint _bytesTotal;
        uint _bytesProgress;

        public AttachmentServiceEventArgs(UploadAttachment attachment): this(attachment, 0, 0)
        {
            
        }

        public AttachmentServiceEventArgs(UploadAttachment attachment, uint bytesTotal, uint bytesProgress)
        {
            _attachment = attachment;
            _bytesTotal = bytesTotal;
            _bytesProgress = bytesProgress;
        }

        public uint BytesTotal { get { return _bytesTotal; } }
        public uint BytesProgress { get { return _bytesProgress; } }
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
        public AttachmentUploadException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public interface IUploadService
    {
        string ServiceName { get; }
        UploadCapabilities Capabilities { get; }
        int UriLength { get; }
        List<MediaType> FileTypes { get; }
        
        // Uploads the attachment. Must set the Uri on success
        void UploadAttachment(UploadAttachment Attachment);

        event AttachmentEventHandler UploadFinish;
        event AttachmentEventHandler ErrorOccured;
        event AttachmentEventHandler UploadPart;
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
/*            IPictureService Posterous = PictureServiceFactory.Instance.GetServiceByName("Posterous");
            if (this.PictureService == null)
                this.PictureService = Posterous;
            if (this.VideoService == null)
                this.VideoService = Posterous;
            if (this.AudioService == null)
                this.AudioService = Posterous;
            if (this.DocumentService == null)
                this.DocumentService = Posterous;*/
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
            if(Attachments.IndexOf(Attachment) < 0)
                Attachments.Add(Attachment);
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
            set { Attachments[i] = value; }
        }

        // Solely designed to convert from AttachmentUploadManager to
        // IPictureService until we get IMediaServices up and running
        // Let's do it one step at a time!
        private bool DoUpload(UploadAttachment a)
        {
            PicturePostObject ppo = new PicturePostObject();
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

            ppo.ContentType = type.ContentType;
            if(a.Position != null)
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
            return null;
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
