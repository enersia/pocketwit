using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Web;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace PockeTwit.MediaServices
{
    public class Multipart
    {
        public delegate void UploadPartEvent(object sender, long bytesSent, long bytesTotal);
        protected class MultipartFile
        {
            public Stream DataStream;
            public string FileName;
            public string ContentType;

            public MultipartFile(Stream ds, string fn, string ct)
            {
                DataStream = ds;
                FileName = fn;
                ContentType = ct;
            }
        }
        List<KeyValuePair<string, string>> Fields = new List<KeyValuePair<string, string>>();
        List<KeyValuePair<string, MultipartFile>> Files = new List<KeyValuePair<string, MultipartFile>>();

        public event UploadPartEvent UploadPart;

        private void OnUploadPart(long bytesSent, long bytesTotal)
        {
            if (UploadPart != null)
            {
                try
                {
                    UploadPart(this, bytesSent, bytesTotal);
                }
                catch
                {}
            }
        }

        public void Add(string key, string value)
        {
            Fields.Add(new KeyValuePair<string,string>(key, value));
        }

        public void Add(string key, Stream data)
        {
            Add(key, data, "", "");
        }

        public void Add(string key, Stream data, string fileName)
        {
            Add(key, data, fileName, null);
        }
        
        public void Add(string key, Stream data, string fileName, string contentType)
        {
            Files.Add(new KeyValuePair<string, MultipartFile>(key, new MultipartFile(data, fileName, contentType)));
        }

        public XmlDocument UploadXML(HttpWebRequest request)
        {
            Upload(request);
            using (WebResponse response = request.GetResponse())
            {
                try
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        XmlDocument responseXML = new XmlDocument();
                        string resp = reader.ReadToEnd();
                        reader.Close();
                        responseXML.LoadXml(resp);
                        return responseXML;
                    }                        
                }
                finally
                {
                    response.Close();
                }

            }
        }
        
        public void Upload(HttpWebRequest request)
        {
            string boundary = System.Guid.NewGuid().ToString();
            request.Method = "POST";
            request.PreAuthenticate = true;
            request.ContentType = string.Format("multipart/form-data;boundary={0}", boundary);

            string header = string.Format("--{0}", boundary);
            string ender = "\r\n" + header + "\r\n";

            StringBuilder contents = new StringBuilder();
            long ContentLength = 0;
            int FileHeaderLength = 0;
            foreach(KeyValuePair<string, string> kvp in Fields)
            {
                contents.Append(Multipart.CreateContentPartString(header, kvp.Key, kvp.Value));
            }

            List<string> FileContentHeaders = new List<string>(Files.Count);
            foreach(KeyValuePair<string, MultipartFile> kvp in Files)
            {
                string contentPart = CreateContentPartPicture(header, kvp.Key, kvp.Value.FileName, kvp.Value.ContentType);
                FileHeaderLength += Encoding.UTF8.GetByteCount(contentPart);
                FileContentHeaders.Add(contentPart);
            }

            //Create the form message to send in bytes
            byte[] message = Encoding.UTF8.GetBytes(contents.ToString());
            byte[] footer = Encoding.UTF8.GetBytes(ender);
            try
            {
                foreach(KeyValuePair<string, MultipartFile> kvp in Files)
                {
                    ContentLength += kvp.Value.DataStream.Length;
                }

                ContentLength += message.Length + footer.Length + FileHeaderLength;
                request.ContentLength = ContentLength;
            }
            catch(NotSupportedException /*nse*/)
            {
                request.SendChunked = true;
            }
            using (Stream requestStream = request.GetRequestStream())
            {
                // send normal form data
                int read;
                long total = 0;
                OnUploadPart(total, ContentLength);// started
                if (message.Length != 0)
                {
                    requestStream.Write(message, 0, message.Length);
                    total += message.Length;
                    OnUploadPart(total, ContentLength);
                }
                
                // now send each file
                byte[] buffer = new byte[8192];
                List<string>.Enumerator fchEnum = FileContentHeaders.GetEnumerator();
                foreach(KeyValuePair<string, MultipartFile> kvp in Files)
                {
                    fchEnum.MoveNext();
                    byte[] cHeader = Encoding.UTF8.GetBytes(fchEnum.Current);
                    requestStream.Write(cHeader, 0, cHeader.Length);
                    total += cHeader.Length;
                    while ((read = kvp.Value.DataStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        requestStream.Write(buffer, 0, read);
                        total += read;
                        OnUploadPart(total, ContentLength);
                    }
                }
                requestStream.Write(footer, 0, footer.Length);
                total += footer.Length;
                requestStream.Flush();
                requestStream.Close();
                OnUploadPart(total, ContentLength); // and finished

                buffer = null; // free the buffer
            }

        }

        #region helper functions

        public static string CreateContentPartString(string header, string dispositionName, string valueToSend)
        {
            StringBuilder contents = new StringBuilder();

            contents.Append(header);
            contents.Append("\r\n");
            contents.Append(String.Format("Content-Disposition: form-data; name=\"{0}\"\r\n", dispositionName));
            contents.Append("\r\n");
            contents.Append(valueToSend);
            contents.Append("\r\n");

            return contents.ToString();
        }

        public static string CreateContentPartPicture(string header, string name, string filename)
        {
            return CreateContentPartPicture(header, name, filename, null);
        }

        public static string CreateContentPartPicture(string header, string name, string filename, string contentType)
        {
            StringBuilder contents = new StringBuilder();

            contents.Append(header);
            contents.Append("\r\n");
            contents.Append(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n", name, filename));
            if(!String.IsNullOrEmpty(contentType))            
                contents.Append(String.Format("Content-Type: {0}\r\n", contentType));
            contents.Append("\r\n");

            return contents.ToString();
        }

        
        #endregion

    }
}