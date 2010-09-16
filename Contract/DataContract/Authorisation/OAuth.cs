using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace DataContract.Authorisation
{
    public class OAuth
    {
        private string _consumerKey = string.Empty;
        private string _callback = string.Empty;
        private string _consumerSecret = string.Empty;
        private string _requestTokenUrl = string.Empty;
        private string _accessTokenUrl = string.Empty;
        private string _authorizeUrl = string.Empty;
        private string _verifyUrlJson = string.Empty;
        private string _verifyUrlXml = string.Empty;

        public string ConsumerKey
        {
            get
            {
                return _consumerKey;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _consumerKey = value;
                }
                else
                {
                    _consumerKey = string.Empty;
                }
            }
        }

        public string Callback
        {
            get
            {
                return _callback;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _callback = value;
                }
                else
                {
                    _callback = string.Empty;
                }
            }
        }

        public string ConsumerSecret
        {
            get
            {
                return _consumerSecret;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _consumerSecret = value;
                }
                else
                {
                    _consumerSecret = string.Empty;
                }
            }
        }

        public string RequestTokenUrl
        {
            get
            {
                return _requestTokenUrl;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _requestTokenUrl = value;
                }
                else
                {
                    _requestTokenUrl = string.Empty;
                }
            }
        }

        public string AccessTokenUrl
        {
            get
            {
                return _accessTokenUrl;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _accessTokenUrl = value;
                }
                else
                {
                    _accessTokenUrl = string.Empty;
                }
            }
        }

        public string AuthorizeUrl
        {
            get
            {
                return _authorizeUrl;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _authorizeUrl = value;
                }
                else
                {
                    _authorizeUrl = string.Empty;
                }
            }
        }

        public string VerifyUrlJson
        {
            get
            {
                return _verifyUrlJson;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _verifyUrlJson = value;
                }
                else
                {
                    _verifyUrlJson = string.Empty;
                }
            }
        }

        public string VerifyUrlXml
        {
            get
            {
                return _verifyUrlXml;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _verifyUrlXml = value;
                }
                else
                {
                    _verifyUrlXml = string.Empty;
                }
            }
        }

    }
}
