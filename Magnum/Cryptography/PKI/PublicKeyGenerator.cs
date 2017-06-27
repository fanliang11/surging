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
    using System.Security.Cryptography;

    public class PublicKeyGenerator
    {
        public RSACryptoServiceProvider GetRsa()
        {
            // RSA wants to store key info in user's account
            // we want to use the local (machine) account instead
            var param = new CspParameters
                        {
                            Flags = CspProviderFlags.UseMachineKeyStore
                        };
            var rsa = new RSACryptoServiceProvider(param);

            return rsa;
        }

        public KeyPair MakeKeyPair()
        {
            RSA rsa = GetRsa();

            var objPublicKey = new PublicKey(rsa.ToXmlString(false));
            var objPrivateKey = new PrivateKey(rsa.ToXmlString(true));
            var keyPair = new KeyPair(objPublicKey, objPrivateKey);

            return keyPair;
        }
    }
}