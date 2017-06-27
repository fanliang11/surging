namespace Magnum.Cryptography.PKI
{
    using System;

    public interface IPkiCryptographyService :
        IDisposable
    {
        string SignAndEncrypt(KeyPair encryptionKeyPair, string plainText);
        string Sign(KeyPair signingKeyPair, string text);
        string Encrypt(KeyPair encryptionKeyPair, string plainText);

        string DecryptAndAuthenticate(KeyPair decryptionKeyPair, string cipherText);
        string Decrypt(KeyPair decryptionKeyPair, string cipherText);
        bool Authenticate(KeyPair authenticationKeyPair, string signedText);

        //EncryptOutput Encrypt(EncryptInput input);
        //DecryptOutput Decrypt(DecryptInput input);

        //generate key pair?
    }
}