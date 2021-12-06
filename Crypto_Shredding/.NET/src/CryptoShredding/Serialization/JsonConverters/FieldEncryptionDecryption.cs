using System;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;

namespace CryptoShredding.Serialization.JsonConverters;

public class FieldEncryptionDecryption
{
    private const string EncryptionPrefix = "crypto.";
        
    public object GetEncryptedOrDefault(object value, ICryptoTransform encryptor)
    {
        if (encryptor is null)
        {
            throw new ArgumentNullException(nameof(encryptor));
        }
        var isEncryptionNeeded = value != null;
        if (isEncryptionNeeded)
        {
            using var memoryStream = new MemoryStream();
            using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
            using var writer = new StreamWriter(cryptoStream);
            var valueAsText = value.ToString();
            writer.Write(valueAsText);
            writer.Flush();
            cryptoStream.FlushFinalBlock();

            var encryptedData = memoryStream.ToArray();
            var encryptedText = Convert.ToBase64String(encryptedData);
            var result = $"{EncryptionPrefix}{encryptedText}";
            return result;
        }

        return default;
    }

    public object GetDecryptedOrDefault(object value, ICryptoTransform decryptor, Type destinationType)
    {
        if (value is null)
        {
            throw new ArgumentNullException(nameof(value));
        }
        var isText = value is string;
        if (isText)
        {
            var valueAsText = (string)value;
            var isEncrypted = valueAsText.StartsWith(EncryptionPrefix);
            if (isEncrypted)
            {
                var isDecryptorAvailable = decryptor != null;
                if (isDecryptorAvailable)
                {
                    var startIndex = EncryptionPrefix.Length;
                    var valueWithoutPrefix = valueAsText.Substring(startIndex);
                    var encryptedValue = Convert.FromBase64String(valueWithoutPrefix);
                    using var memoryStream = new MemoryStream(encryptedValue);
                    using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
                    using var reader = new StreamReader(cryptoStream);
                    var decryptedText = reader.ReadToEnd();
                    var result = Parse(destinationType, decryptedText);
                    return result;
                }
                var maskedValue = GetMaskedValue(destinationType);
                return maskedValue;
            }
            var valueParsed = Parse(destinationType, valueAsText);
            return valueParsed;
        }

        return value;
    }

    private object Parse(Type outputType, string valueAsString)
    {
        var converter = TypeDescriptor.GetConverter(outputType);
        var result = converter.ConvertFromString(valueAsString);
        return result;
    }
        
    private object GetMaskedValue(Type destinationType)
    {
        if (destinationType == typeof(string))
        {
            const string templateText = "***";
            return templateText;
        }
        var defaultValue = Activator.CreateInstance(destinationType);
        return defaultValue;
    }
}