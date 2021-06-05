using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Mix.Cms.Lib.Helpers
{
    public class RSAEncryptionHelper
    {
        private static UnicodeEncoding ByteConverter = new UnicodeEncoding();

        public static Dictionary<string, string> GenerateKeys()
        {
            //lets take a new CSP with a new 2048 bit rsa key pair
            var csp = new RSACryptoServiceProvider(2048);

            //how to get the private key
            var privKey = csp.ExportParameters(true);

            //and the public key ...
            var pubKey = csp.ExportParameters(false);

            //get the string from the stream
            string pubKeyString = GetKeyString(pubKey);
            string privKeyString = GetKeyString(privKey);

            return new Dictionary<string, string>()
            {
                ["PrivateKey"] = privKeyString,
                ["PublicKey"] = pubKeyString
            };
        }

        private static string GetKeyString(RSAParameters key)
        {
            var sw = new System.IO.StringWriter();
            //we need a serializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //serialize the key into the stream
            xs.Serialize(sw, key);
            return sw.ToString();
        }

        private static RSAParameters GetKey(string keyString)
        {
            //get a stream from the string
            var sr = new System.IO.StringReader(keyString);
            //we need a deserializer
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            //get the object back from the stream
            return (RSAParameters)xs.Deserialize(sr);
        }

        public static string GetEncryptedText(string plainTextData, string pubKeyString)
        {
            RSAParameters pubKey = GetKey(pubKeyString);
            //we have a public key ... let's get a new csp and load that key
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(pubKey);

            var bytesPlainTextData = System.Text.Encoding.Unicode.GetBytes(plainTextData);

            //apply pkcs#1.5 padding and encrypt our data 
            var bytesCypherText = csp.Encrypt(bytesPlainTextData, false);

            //we might want a string representation of our cypher text... base64 will do
            return Convert.ToBase64String(bytesCypherText);
        }

        public static string GetDecryptedText(string cypherText, string privKeyString)
        {
            RSAParameters privKey = GetKey(privKeyString);
            var bytesCypherText = Convert.FromBase64String(cypherText);

            //we want to decrypt, therefore we need a csp and load our private key
            var csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privKey);

            //decrypt and strip pkcs#1.5 padding
            var bytesPlainTextData = csp.Decrypt(bytesCypherText, false);

            //get our original plainText back...
            return Encoding.Unicode.GetString(bytesPlainTextData);
        }

    }
}