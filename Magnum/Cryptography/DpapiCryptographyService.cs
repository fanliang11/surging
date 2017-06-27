// Copyright 2007-2010 The Apache Software Foundation.
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
namespace Magnum.Cryptography
{
    using System.IO;
    using System.Security.Cryptography;
    using System.Text;
    using Extensions;

    public class DpapiCryptographyService :
        ICryptographyService
    {
        public void Dispose()
        {
            //no-op
        }

        public EncryptedText Encrypt(string clearText)
        {
            byte[] clearBytes = Encoding.UTF8.GetBytes(clearText);
            byte[] iv = GenerateIv();
            byte[] cipherBytes = ProtectedData.Protect(clearBytes, iv, DataProtectionScope.LocalMachine);
            var result = new EncryptedText(cipherBytes, iv);
            return result;
        }

        public string Decrypt(EncryptedText cipherText)
        {
            byte[] cipherBytes = cipherText.GetBytes();
            byte[] iv = cipherText.Iv;
            byte[] clearBytes = ProtectedData.Unprotect(cipherBytes, iv, DataProtectionScope.LocalMachine);
            return Encoding.UTF8.GetString(clearBytes);
        }

        public EncryptedStream Encrypt(Stream clearStream)
        {
            byte[] clearBytes = clearStream.ReadToEnd();
            byte[] iv = GenerateIv();
            byte[] cipherBytes = ProtectedData.Protect(clearBytes, iv, DataProtectionScope.LocalMachine);
            var result = new EncryptedStream(cipherBytes, iv);
            return result;
        }

        public Stream Decrypt(EncryptedStream cipherStream)
        {
            byte[] cipherBytes = cipherStream.GetBytes();
            byte[] iv = cipherStream.Iv;
            byte[] clearBytes = ProtectedData.Unprotect(cipherBytes, iv, DataProtectionScope.LocalMachine);
            return new MemoryStream(clearBytes);
        }

        byte[] GenerateIv()
        {
            using (var c = new RijndaelManaged
                               {
                                   KeySize = 256,
                                   // defaults to 256, it's better to be explicit.
                                   BlockSize = 256,
                                   // defaults to 128 bits, so let's set this to 256 for better security
                                   Mode = CipherMode.CBC,
                                   Padding = PaddingMode.ISO10126,
                                   // adds random padding bytes which reduces the predictability of the plain text
                               })
            {
                c.GenerateIV();
                return c.IV;
            }
        }
    }
}