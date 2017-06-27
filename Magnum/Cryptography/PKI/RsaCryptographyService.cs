// Copyright 2007-2008 The Apache Software Foundation.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Magnum.Cryptography.PKI
{
    using System;
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.RegularExpressions;

    //http://www.devx.com/security/Article/17249/0/page/3
    public class RsaCryptographyService :
        IPkiCryptographyService
    {
        readonly RSACryptoServiceProvider _rsaCryptoServiceProvider;

        public RsaCryptographyService()
        {
            var publicKeyGenerator = new PublicKeyGenerator();
            _rsaCryptoServiceProvider = publicKeyGenerator.GetRsa();
        }

        // We use Direct Encryption (PKCS#1 v1.5) - so we require MS Windows 2000 or later with high encryption pack installed.

        public string SignAndEncrypt(KeyPair encryptionKeyPair, string plainText)
        {
            // Use Public Key to encrypt and private key to sign
            var signed = Sign(encryptionKeyPair, plainText);
            var enc = Encrypt(encryptionKeyPair, signed);
            return enc;
        }
        public string Sign(KeyPair signingKeyPair, string text)
        {
            //Use PrivateKey to sign
            _rsaCryptoServiceProvider.FromXmlString(signingKeyPair.Private.Key);
            var signedData = _rsaCryptoServiceProvider.SignData(TextHelpers.ClearTextToClearBytes(text), HashAlgorithm.Create());
            var signature = TextHelpers.CipherBytesToCipherText(signedData);
            return string.Format("{0}<signature>{1}</signature>", text, signature);
        }
        public string Encrypt(KeyPair encryptionKeyPair, string plainText)
        {
            //use THEIR public key to encrypt
            _rsaCryptoServiceProvider.FromXmlString(encryptionKeyPair.Public.Key);

            //Get Modulus Size and compare it to length of PlainText
            // If Length of PlainText > (Modulus Size - 11), then PlainText will need to be broken into segments of size (Modulus Size - 11)
            // Each of these segments will be encrypted separately
            //     and will return encrypted strings equal to the Modulus Size (with at least 11 bytes of padding)
            // When decrypting, if the EncryptedText string > Modulus size, it will be split into segments of size equal to Modulus Size
            // Each of these EncryptedText segments will be decrypted individually with the resulting PlainText segments re-assembled.

            var blockSize = GetModulusSize() - 11;
            var plainStream = new MemoryStream(TextHelpers.ClearTextToClearBytes(plainText));
            var cipherStream = new MemoryStream();
            var buffer = new byte[blockSize];

            while(plainStream.Read(buffer, 0, blockSize) > 0)
            {
                var c = _rsaCryptoServiceProvider.Encrypt(buffer, false);
                cipherStream.Write(c, 0, c.Length);
            }

            var cipherBytes = cipherStream.ToArray();
            return TextHelpers.CipherBytesToCipherText(cipherBytes);
        }
        
        public string DecryptAndAuthenticate(KeyPair decryptionKeyPair, string cipherText)
        {
            //Use Private key to Decrypt and Public Key to Authenticate

            var plainText = Decrypt(decryptionKeyPair, cipherText);
            if (!Authenticate(decryptionKeyPair, plainText))
            {
                throw new Exception("Message authentication failed.");
            }

            return CryptoHelpers.StripSignature(plainText);
        }
        public string Decrypt(KeyPair decryptionKeyPair, string cipherText)
        {
            //they use THEIR private key to decrypt
            _rsaCryptoServiceProvider.FromXmlString(decryptionKeyPair.Private.Key);

            var blockSize = GetModulusSize();
            var plainStream = new MemoryStream();
            var cipherStream = new MemoryStream(TextHelpers.CipherTextToCipherBytes(cipherText));
            var buffer = new byte[blockSize];

            int r;
            while((r=cipherStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                var p = _rsaCryptoServiceProvider.Decrypt(buffer, false);
                plainStream.Write(p, 0, p.Length);
            }
            //TODO: getting extra data here. not sure why
            var clearBytes = plainStream.ToArray();
            return TextHelpers.ClearBytesToClearString(clearBytes);
        }
        public bool Authenticate(KeyPair authenticationKeyPair, string signedText)
        {
            _rsaCryptoServiceProvider.FromXmlString(authenticationKeyPair.Public.Key);
            string signature = CryptoHelpers.ExtractSignature(signedText);
            string message = CryptoHelpers.StripSignature(signedText);

            if (string.IsNullOrEmpty(signature))
            {
                throw new Exception("Digital signature is missing or not formatted properly.");
            }

            var bytes = TextHelpers.ClearTextToClearBytes(message);
            var sigbytes = TextHelpers.CipherTextToCipherBytes(signature);

            return _rsaCryptoServiceProvider.VerifyData(bytes, HashAlgorithm.Create(), sigbytes);
        }


        int GetModulusSize()
        {
            //in bytes
            return (int)Math.Round(_rsaCryptoServiceProvider.KeySize / 8.0);
        }

        public void Dispose()
        {
            _rsaCryptoServiceProvider.Clear();
        }
    }

    public class CryptoHelpers
    {
        public static string StripSignature(string signedText)
        {
            var regex = new Regex(@"(?<sigtag>\<signature\>(?<signature>.+)\<\/signature\>.*)");
            //                                                                            ^why is there extra data?
            var result = regex.Match(signedText);
            
            return signedText.Replace(result.Groups["sigtag"].Value,"");
        }
        public static string ExtractSignature(string signedText)
        {
            var regex = new Regex(@"(?<sigtag>\<signature\>(?<signature>.+)\<\/signature\>)");
            var r = regex.Match(signedText);
            return r.Groups["signature"].Value;
        }
    }

    public class TextHelpers
    {
        public static byte[] ClearTextToClearBytes(string input)
        {
            return Encoding.UTF8.GetBytes(input);
        }

        public static string ClearBytesToClearString(byte[] input)
        {
            return Encoding.UTF8.GetString(input);
        }

        public static byte[] CipherTextToCipherBytes(string input)
        {
            return Convert.FromBase64String(input);
        }

        public static string CipherBytesToCipherText(byte[] input)
        {
            return Convert.ToBase64String(input);
        }

    }

    public class TestIt
    {
        public void Bob()
        {
            var aliceKeyPair = new PublicKeyGenerator().MakeKeyPair();
            var bobKeyPair = new PublicKeyGenerator().MakeKeyPair();
            var crypto = new RsaCryptographyService();

            var message = "ch";

            //message from bob to alice - encrypt with alice public key - sign with bob's private key
            var keyPair = new KeyPair(aliceKeyPair.Public, bobKeyPair.Private);

            var cipherText = crypto.SignAndEncrypt(keyPair, message);
            Console.WriteLine(cipherText);

            //message received - decrypt with alice private - verify with bob's public key
            var k2 = new KeyPair(bobKeyPair.Public, aliceKeyPair.Private);
            var clear = crypto.DecryptAndAuthenticate(k2, cipherText);
            Console.WriteLine(clear);
        }

    }
}