using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Security.Cryptography;

namespace Yedda
{
    
    public class TrustOAuthCertificates : System.Net.ICertificatePolicy
    {
        public TrustOAuthCertificates()
        { 
        }

        public bool CheckValidationResult(ServicePoint sp, System.Security.Cryptography.X509Certificates.X509Certificate cert, WebRequest req, int problem)
        {
            //We really want to check the cert and see if it matches the
            //servers we expect, but this lets me keep going for now
            return true;

        }
    }
}
