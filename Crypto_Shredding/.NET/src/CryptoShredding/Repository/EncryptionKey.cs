namespace CryptoShredding.Repository
{
    public class EncryptionKey
    {
        public byte[] Key { get; }
        public byte[] Nonce { get; }
        
        public EncryptionKey(
            byte[] key,
            byte[] nonce)
        {
            Key = key;
            Nonce = nonce;
        }
    }
}