namespace Magnum.Cryptography.PKI
{
    public class PrivateKey
    {
        public PrivateKey(string key)
        {
            Key = key;
        }

        public string Label { get; set; }
        public string Key { get; set; }
    }

    public class PublicKey
    {
        public PublicKey(string key)
        {
            Key = key;
        }

        public string Label { get; set; }
        public string Key { get; set; }
    }

    public class EncryptInput
    {
        public string ClearText { get; set; }
        public PrivateKey SourceKey { get; set; }
        public PublicKey DestinationKey { get; set; }
    }
    public class EncryptOutput
    {
        public string CipherText { get; set; }
    }
    public class DecryptInput
    {
        public string CipherText { get; set; }
        public PrivateKey DestinationKey { get; set; }
        public PublicKey SourceKey { get; set; }
    }
    public class DecryptOutput
    {
        public string ClearText { get; set; }
    }
}