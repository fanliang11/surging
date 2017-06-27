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
    using System;
    using System.Security.Cryptography;
    using System.Text;

    public class Sha512HMacHashingService :
        MacHashingService
    {
        readonly byte[] _key;
        readonly Encoding _encoding;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key">The key to use in the hashing algorhythem. Can be of any length</param>
        public Sha512HMacHashingService(string key)
        {
            _encoding = Encoding.UTF8;
            _key = _encoding.GetBytes(key);
        }

        public string Hash(string clearText)
        {
            byte[] hashBytes = Hash(_encoding.GetBytes(clearText));
            return Convert.ToBase64String(hashBytes);
        }

        public byte[] Hash(byte[] clearBytes)
        {
            using (var hmac = new HMACSHA512(_key))
            {
                hmac.Initialize();
                byte[] hashBytes = hmac.ComputeHash(clearBytes);
                return hashBytes;
            }
        }
    }
}