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

    //http://www.developerfusion.co.uk/show/4647/4/

    public class RijndaelCryptographyService :
        ICryptographyService
    {
        //a block cipher adopted as an encryption
        //standard by the U.S. government.
        //http://en.wikipedia.org/wiki/Rijndael
        readonly RijndaelManaged _cipher;

        public RijndaelCryptographyService(string key)
        {
            _cipher = new RijndaelManaged
                          {
                              KeySize = 256,
                              // defaults to 256, it's better to be explicit.
                              BlockSize = 256,
                              // defaults to 128 bits, so let's set this to 256 for better security
                              Mode = CipherMode.CBC,
                              Padding = PaddingMode.ISO10126,
                              // adds random padding bytes which reduces the predictability of the plain text
                          };

            _cipher.Key = Encoding.UTF8.GetBytes(key);
        }

        public EncryptedText Encrypt(string clearText)
        {
            _cipher.GenerateIV();

            using (ICryptoTransform transform = _cipher.CreateEncryptor())
            {
                byte[] clearBytes = Encoding.UTF8.GetBytes(clearText);

                byte[] cipherBytes = transform.TransformFinalBlock(clearBytes, 0, clearBytes.Length);

                return new EncryptedText(cipherBytes, _cipher.IV);
            }
        }

        public EncryptedStream Encrypt(Stream clearStream)
        {
            _cipher.GenerateIV();

            using (ICryptoTransform t = _cipher.CreateEncryptor())
            {
                byte[] clearBytes = clearStream.ReadToEnd();

                byte[] cipherBytes = t.TransformFinalBlock(clearBytes, 0, clearBytes.Length);

                return new EncryptedStream(cipherBytes, _cipher.IV);
            }
        }

        public string Decrypt(EncryptedText cipherText)
        {
            _cipher.IV = cipherText.Iv;

            using (ICryptoTransform transform = _cipher.CreateDecryptor())
            {
                byte[] cipherBytes = cipherText.GetBytes();

                byte[] clearBytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                return Encoding.UTF8.GetString(clearBytes);
            }
        }

        public Stream Decrypt(EncryptedStream cipherStream)
        {
            _cipher.IV = cipherStream.Iv;

            using (ICryptoTransform transform = _cipher.CreateDecryptor())
            {
                byte[] cipherBytes = cipherStream.GetBytes();

                byte[] clearBytes = transform.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

                return new MemoryStream(clearBytes);
            }
        }

        public void Dispose()
        {
            _cipher.Clear();
        }
    }
}