namespace MervaApi.Encryption.Services;

public interface IEncryptionService
{
    string Encrypt(string plaintext);
    string Decrypt(string ciphertext);
    byte[] ComputeSha256(string value);
}
