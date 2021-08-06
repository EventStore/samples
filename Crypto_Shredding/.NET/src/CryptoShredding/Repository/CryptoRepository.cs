using System;
using System.Collections.Generic;

namespace CryptoShredding.Repository
{
    public class CryptoRepository
    {
        private readonly IDictionary<string, EncryptionKey> _cryptoStore;

        public CryptoRepository()
        {
            _cryptoStore = new Dictionary<string, EncryptionKey>();
        }

        public EncryptionKey GetExistingOrNew(string id, Func<EncryptionKey> keyGenerator)
        {
            var isExisting = _cryptoStore.TryGetValue(id, out var keyStored);
            if (isExisting)
            {
                return keyStored;
            }

            var newEncryptionKey = keyGenerator.Invoke();
            _cryptoStore.Add(id, newEncryptionKey);
            return newEncryptionKey;
        }
        
        public EncryptionKey GetExistingOrDefault(string id)
        {
            var isExisting = _cryptoStore.TryGetValue(id, out var keyStored);
            if (isExisting)
            {
                return keyStored;
            }

            return default;
        }

        public void DeleteEncryptionKey(string id)
        {
            _cryptoStore.Remove(id);
        }
    }
}