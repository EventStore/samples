using System.Security.Cryptography;
using CryptoShredding.Repository;

namespace CryptoShredding.Serialization
{
    public class EncryptorDecryptor
    {
        private readonly CryptoRepository _cryptoRepository;

        public EncryptorDecryptor(CryptoRepository cryptoRepository)
        {
            _cryptoRepository = cryptoRepository;
        }
        
        public ICryptoTransform GetEncryptor(string dataSubjectId)
        {
            var encryptionKey = _cryptoRepository.GetExistingOrNew(dataSubjectId, CreateNewEncryptionKey);
            var aesManaged = GetAesManaged(encryptionKey);
            var encryptor = aesManaged.CreateEncryptor();
            return encryptor;
        }

        public ICryptoTransform GetDecryptor(string dataSubjectId)
        {
            var encryptionKey = _cryptoRepository.GetExistingOrDefault(dataSubjectId);
            if (encryptionKey is null)
            {
                // encryption key was deleted
                return default;
            }
            
            var aesManaged = GetAesManaged(encryptionKey);
            var decryptor = aesManaged.CreateDecryptor();
            return decryptor;
        }
        
        private EncryptionKey CreateNewEncryptionKey()
        {
            var aesManaged =
                new AesManaged
                {
                    Padding = PaddingMode.PKCS7
                };
            var key = aesManaged.Key;
            var nonce = aesManaged.IV;
            var encryptionKey = new EncryptionKey(key, nonce);
            return encryptionKey;
        }
        
        private AesManaged GetAesManaged(EncryptionKey encryptionKey)
        {
            var aesManaged =
                new AesManaged
                {
                    Padding = PaddingMode.PKCS7,
                    Key = encryptionKey.Key,
                    IV = encryptionKey.Nonce
                };
            
            return aesManaged;
        }
    }
}