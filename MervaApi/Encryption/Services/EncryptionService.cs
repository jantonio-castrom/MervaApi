using System.Security.Cryptography;
using System.Text;

namespace MervaApi.Encryption.Services;

public class EncryptionService : IEncryptionService
{
    private readonly byte[] _key;

    public EncryptionService(IConfiguration configuration)
    {
        var keyBase64 = configuration["USERTOKEN_KEY"]
            ?? throw new InvalidOperationException("Encryption:Key is not configured.");

        _key = Convert.FromBase64String(keyBase64);

        if (_key.Length != 32)
            throw new InvalidOperationException("Encryption:Key must be a 32-byte (256-bit) Base64-encoded value.");
    }

    public string Encrypt(string plaintext)
    {
        using var aes = Aes.Create();
        aes.Key = _key;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
        var ciphertextBytes = encryptor.TransformFinalBlock(plaintextBytes, 0, plaintextBytes.Length);
        
        var result = new byte[aes.IV.Length + ciphertextBytes.Length];
        aes.IV.CopyTo(result, 0);
        ciphertextBytes.CopyTo(result, aes.IV.Length);

        return Convert.ToBase64String(result);
    }

    public string Decrypt(string ciphertext)
    {
        var data = Convert.FromBase64String(ciphertext);

        using var aes = Aes.Create();
        aes.Key = _key;
        aes.IV = data[..16];

        using var decryptor = aes.CreateDecryptor();
        var plaintextBytes = decryptor.TransformFinalBlock(data, 16, data.Length - 16);
        return Encoding.UTF8.GetString(plaintextBytes);
    }

    public byte[] ComputeSha256(string value)
    {
        using var sha = SHA256.Create();
        return sha.ComputeHash(
        Encoding.UTF8.GetBytes(value)
        );
    }
}
