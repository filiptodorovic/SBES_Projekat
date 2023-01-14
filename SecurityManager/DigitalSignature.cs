using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace SecurityManager
{
    public class DigitalSignature
    {

        // default hash is SHA256
        public static byte[] Create(byte[] message, X509Certificate2 certificate)
        {
            RSACryptoServiceProvider csp = (RSACryptoServiceProvider)certificate.PrivateKey;

            if (csp == null)
            {
                throw new Exception("Valid certificate was not found.");
            }

            byte[] data = message;
            byte[] hash = null;
            
            MD5 md5 = MD5.Create();
            hash = md5.ComputeHash(data);
            
            /// Use RSACryptoServiceProvider support to create a signature using a previously created hash value
            byte[] signature = csp.SignHash(hash, CryptoConfig.MapNameToOID("MD5"));
            return signature;
        }


        public static bool Verify(byte[] message, byte[] signature, X509Certificate2 certificate)
        {
            RSACryptoServiceProvider csp = (RSACryptoServiceProvider)certificate.PublicKey.Key;

            UnicodeEncoding encoding = new UnicodeEncoding();
            byte[] data = message;
            byte[] hash = null;

            MD5 md5 = MD5.Create();
            hash = md5.ComputeHash(data);

            /// Use RSACryptoServiceProvider support to compare two - hash value from signature and newly created hash value
            return csp.VerifyHash(hash, CryptoConfig.MapNameToOID("MD5"), signature);
        }
    }
}
