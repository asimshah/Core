using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.IO;

namespace Fastnet.Core
{
    /*
     * IMPORTANT NOTES
     * 
     * EncryptionHelper contains exactly the same code as StringCipher in the fastnet private dll except that
     * the Encrypt/Decrypt methods take a passPhrase rather than one of the Fastnet.Core.ApplicationKeys
     * 
     * The idea is that StringCipher will be removed  from the private dll and the private static string GetPassphrase(string appkey) made public
     * so that the only dotfuscated call will be the GetPassphrase(string appkey). QPara mailchimp will need to be altered to this call to
     * obtain the passphrase and then call the Decrypt method here ...
     * 
     * All this to allow wider use of the Encrypt/Decrypt methods
     */
    internal static class EncryptionHandler
    {
        // This constant is used to determine the keysize of the encryption algorithm in bits.
        // We divide this by 8 within the code below to get the equivalent number of bytes.
        private const int Keysize = 128;

        // This constant determines the number of iterations for the password bytes generation function.
        private const int DerivationIterations = 1000;

        internal static string Encrypt(string plainText, string passphrase)
        {
            // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
            // so that the same Salt and IV values can be used when decrypting.  
            var saltStringBytes = GenerateBitsOfRandomEntropy(16);
            var ivStringBytes = GenerateBitsOfRandomEntropy(16);
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passphrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                // Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        internal static string Decrypt(string cipherText, string passphrase)
        {
            // Get the complete stream of bytes that represent:
            // [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
            // Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();
            // Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();
            // Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passphrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 128;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] GenerateBitsOfRandomEntropy(int size)
        {
            // 32 Bytes will give us 256 bits.
            // 16 Bytes will give us 128 bits.
            var randomBytes = new byte[size];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                // Fill the array with cryptographically secure random bytes.
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
    //internal static class _secExtensions
    //{
    //    public static byte[] StringToByteArray(this string hex)
    //    {
    //        return Enumerable.Range(0, hex.Length)
    //            .Where(x => x % 2 == 0)
    //            .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
    //            .ToArray();
    //    }
    //    public static string ByteArrayToString(this byte[] ba)
    //    {
    //        StringBuilder hex = new StringBuilder(ba.Length * 2);
    //        foreach (byte b in ba)
    //            hex.AppendFormat("{0:x2}", b);
    //        return hex.ToString();
    //    }
    //}
    //internal class PasswordGenerator
    //{
    //    public static string GenerateRandomPassword(int length, string charSet = @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:-_/\!§$%&(){[]}?=@+~*,;<>|", int addHyphensEveryNLetters = 0)
    //    {

    //        // @"ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789.:-_/\!§$%&(){[]}?=@+~*,;<>|";

    //        //charSet = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

    //        var stringChars = new char[length];

    //        for (int i = 0; i < stringChars.Length; i++)
    //        {
    //            stringChars[i] = charSet[Next(0, charSet.Length)];
    //        }

    //        var password = new String(stringChars);
    //        if (addHyphensEveryNLetters != 0)
    //        {
    //            password = String.Join("-", Regex.Split(password, @"(?<=\G.{" + addHyphensEveryNLetters + "})(?!$)"));
    //        }

    //        return password;
    //    }

    //    public static byte[] GetRandomByteArray(int length)
    //    {
    //        return GenerateRandomBytes(length);
    //    }


    //    // maxExclusiveValue means this value is never returned (exclusive)
    //    public static int Next(int minValue, int maxExclusiveValue)
    //    {
    //        if (minValue >= maxExclusiveValue)
    //            throw new ArgumentOutOfRangeException("minValue must be lower than maxExclusiveValue");

    //        long diff = (long)maxExclusiveValue - minValue;
    //        long upperBound = uint.MaxValue / diff * diff;

    //        uint ui;
    //        do
    //        {
    //            ui = GetRandomUInt();
    //        } while (ui >= upperBound);
    //        return (int)(minValue + (ui % diff));
    //    }

    //    private static uint GetRandomUInt()
    //    {
    //        var randomBytes = GenerateRandomBytes(sizeof(uint));
    //        return BitConverter.ToUInt32(randomBytes, 0);
    //    }

    //    private static byte[] GenerateRandomBytes(int bytesNumber)
    //    {
    //        var csp = new RNGCryptoServiceProvider();
    //        byte[] buffer = new byte[bytesNumber];
    //        csp.GetBytes(buffer);
    //        return buffer;
    //    }
    //}
    //internal class EncryptionOptions // this class modified to not use "scrypt" ever!
    //{
    //    public string DerivationType; // "rfc" or "scrypt"
    //    public byte[] Salt;
    //    public byte[] RijndaelIv;
    //    public int Cost;
    //    public int BlockSize;
    //    public int Parallel;
    //    public int KeySizeInBytes; // In bytes, e. g. 32
    //    public int DerivationIterations; // ONly for Rfc2898, not Scrypt, should be 10,000 or above

    //    //public EncryptionOptions(string type = "scrypt", byte[] salt = null, byte[] iVRijndaelIv = null, int cost = 16384, int blockSize = 8, int parallel = 1,
    //    //    int keySizeInBytes = 32, int derivationIterations = 10000)
    //    public EncryptionOptions( byte[] salt = null, byte[] iVRijndaelIv = null, int cost = 16384, int blockSize = 8, int parallel = 1,
    //        int keySizeInBytes = 32, int derivationIterations = 10000)
    //    {

    //        string type = "rfc";
    //        DerivationType =  type; 
    //        if (salt == null)
    //        {
    //            Salt = PasswordGenerator.GetRandomByteArray(32);
    //        }
    //        else
    //        {
    //            Salt = salt;
    //        }

    //        RijndaelIv = iVRijndaelIv;

    //        KeySizeInBytes = keySizeInBytes;

    //        if (type == null || type == "scrypt")
    //        {
    //            throw new Exception("Only DerivationType rfc is supported");
    //            //DerivationType = "scrypt";
    //            //Cost = cost;
    //            //BlockSize = blockSize;
    //            //Parallel = parallel;
    //        }
    //        else //in Case of rfc2898
    //        {
    //            DerivationType = "pbkdf2";
    //            DerivationIterations = derivationIterations;

    //        }


    //    }




    //}
    //internal class CipherResult
    //{
    //    public string DerivationType { get; set; } // "rfc2898" or "scrypt"
    //    public byte[] Salt { get; set; } // In HEX string
    //    public int Cost { get; set; }
    //    public int BlockSize { get; set; }
    //    public int Parallel { get; set; }
    //    public int KeySizeInBytes { get; set; } // In bytes, e. g. 32
    //    public int DerivationIterations { get; set; } // ONly for Rfc2898, not Scrypt, should be 10,000 or above
    //    public byte[] AesRijndaelIv { get; set; }
    //    public byte[] CipherOutput { get; set; }

    //    public CipherResult()
    //    {


    //    }
    //    public CipherResult(EncryptionOptions eO, byte[] cipherText)
    //    {
    //        if (eO != null)
    //        {
    //            DerivationType = eO.DerivationType;
    //            Salt = eO.Salt;
    //            Cost = eO.Cost;
    //            BlockSize = eO.BlockSize;
    //            Parallel = eO.Parallel;
    //            KeySizeInBytes = eO.KeySizeInBytes;
    //            DerivationIterations = eO.DerivationIterations;
    //            CipherOutput = cipherText;
    //            AesRijndaelIv = eO.RijndaelIv;
    //        }

    //    }

    //    public CipherResultText ConvertToCipherTextObject()
    //    {
    //        var cipherTextObject = new CipherResultText()
    //        {
    //            AesRijndaelIv = Convert.ToBase64String(AesRijndaelIv),
    //            BlockSize = BlockSize,
    //            CipherOutputText = (CipherOutput == null) ? null : Convert.ToBase64String(CipherOutput),
    //            Cost = Cost,
    //            DerivationIterations = DerivationIterations,
    //            DerivationType = DerivationType,
    //            KeySizeInBytes = KeySizeInBytes,
    //            Parallel = Parallel,
    //            //Salt = ScryptHandler.ByteArrayToString(Salt)
    //            Salt = Salt.ByteArrayToString()
    //        };
    //        return cipherTextObject;

    //    }
    //}
    //internal class CipherResultText
    //{
    //    public string DerivationType { get; set; } // "rfc2898" or "scrypt"
    //    public string Salt { get; set; } // In HEX string
    //    public int Cost { get; set; }
    //    public int BlockSize { get; set; }
    //    public int Parallel { get; set; }
    //    public int KeySizeInBytes { get; set; } // In bytes, e. g. 32
    //    public int DerivationIterations { get; set; } // ONly for Rfc2898, not Scrypt, should be 10,000 or above
    //    public string AesRijndaelIv { get; set; } // In base64
    //    public string CipherOutputText { get; set; } // In base64


    //    public CipherResultText()
    //    {

    //    }

    //    public CipherResultText(EncryptionOptions eO, byte[] cipherText)
    //    {
    //        if (eO != null)
    //        {
    //            DerivationType = eO.DerivationType;
    //            //Salt = ScryptHandler.ByteArrayToString(eO.Salt);
    //            Cost = eO.Cost;
    //            BlockSize = eO.BlockSize;
    //            Parallel = eO.Parallel;
    //            KeySizeInBytes = eO.KeySizeInBytes;
    //            DerivationIterations = eO.DerivationIterations;
    //            CipherOutputText = Convert.ToBase64String(cipherText);
    //            AesRijndaelIv = Convert.ToBase64String(eO.RijndaelIv);
    //        }

    //    }

    //    public CipherResult ConvertToCipherObject()
    //    {
    //        var cipherObject = new CipherResult()
    //        {
    //            AesRijndaelIv = Convert.FromBase64String(AesRijndaelIv),
    //            BlockSize = BlockSize,
    //            CipherOutput = Convert.FromBase64String(CipherOutputText),
    //            Cost = Cost,
    //            DerivationIterations = DerivationIterations,
    //            DerivationType = DerivationType,
    //            KeySizeInBytes = KeySizeInBytes,
    //            Parallel = Parallel,
    //            //Salt = ScryptHandler.StringToByteArray(Salt)
    //            Salt = Salt.StringToByteArray()
    //        };
    //        return cipherObject;

    //    }
    //}
    //internal static class EncryptionHandler
    //{
    //    /* This method not only returns ciphertext, but also the IV and the SALT, 
    //       which is important for the deciphering on the JS side. Having the same 
    //       IV and same SALT as suggested by demos on Stackoverflow, etc, is detrimental
    //       to the security. IV and SALT can be sent openly alongside ciphertext. An
    //       attacker cannot make anything with IV and SALT without the password.
    //       Explanation: With different IV, the same plaintext always results in different
    //       ciphertexts. With different SALTS, the same password always results in different
    //       ciphertexts. The ciphertext will be a JSON Object:
    //    */
    //    /*
    //    {
    //        "DerivationType": "scrypt", // optionally: rfc
    //        "Salt": "3a069e9126af66a839067f8a272081136d8ce63ed72176dc8a29973d2b15361f", //SALT must be in Hex
    //        "Cost": 16384, //only for DerivationType "scrypt", not for "rfc"
    //        "BlockSize": 8, //only for DerivationType "scrypt", not for "rfc"
    //        "Parallel": 1, //only for DerivationType "scrypt", not for "rfc"
    //        "KeySizeInBytes": 32,
    //        "DerivationIterations": 0 // Only for DerivationType "rfc", not needed for "scrypt"
            
    //    }
    //    */
    //    public static string EncryptToJson(string plainText, string passPhrase/*, EncryptionOptions eO = null*/)
    //    {
    //        // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
    //        // so that the same Salt and IV values can be used when decrypting.  
    //        var cipherObject = BasicAesEncryption(new System.Text.UTF8Encoding().GetBytes(plainText), passPhrase, null /*eO*/);

    //        var cipherWithSaltAndIvObject = cipherObject.ConvertToCipherTextObject();
    //        //string json = JsonConvert.SerializeObject(cipherWithSaltAndIvObject, Formatting.None);
    //        string json = JsonSerializer.Serialize<CipherResultText>(cipherWithSaltAndIvObject);
    //        return json;
    //    }
    //    public static string Encrypt(string plainText, string passPhrase)
    //    {
    //        // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
    //        // so that the same Salt and IV values can be used when decrypting.  
    //        var cipherObject = BasicAesEncryption(new System.Text.UTF8Encoding().GetBytes(plainText), passPhrase, null /*eO*/);

    //        var cipherWithSaltAndIvObject = cipherObject.ConvertToCipherTextObject();
    //        return string.Join("|", cipherWithSaltAndIvObject.AesRijndaelIv, cipherWithSaltAndIvObject.CipherOutputText, cipherWithSaltAndIvObject.Salt);
    //    }
    //    public static CipherResult EncryptToByteArray(string plainText, string passPhrase, EncryptionOptions eO = null)
    //    {
    //        var cipherObject = BasicAesEncryption(new System.Text.UTF8Encoding().GetBytes(plainText), passPhrase, eO);
    //        return cipherObject;
    //    }
    //    public static CipherResult BinaryEncryptWithStaticIv(byte[] fileToEncrypt, string passPhrase,
    //        EncryptionOptions eO = null)
    //    {
    //        var cipherObject = BasicAesEncryption(fileToEncrypt, passPhrase, eO);
    //        return cipherObject;
    //    }
    //    // The resulting CipherResult contains all important settings (options) and the resulting CipherText (in Byte Array)
    //    public static CipherResult BasicAesEncryption(byte[] bytesToEncrypt, string passPhrase, EncryptionOptions eO = null)
    //    {
    //        // Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
    //        // so that the same Salt and IV values can be used when decrypting.  
    //        var myRijndael = new RijndaelManaged();
    //        myRijndael.BlockSize = 128;
    //        if (eO == null)
    //        {
    //            eO = new EncryptionOptions();
    //            myRijndael.IV = GenerateXBytesOfRandomEntropy(16); //IV must be 16 bytes / 128 bit
    //            eO.RijndaelIv = myRijndael.IV;
    //        }
    //        else if (eO.RijndaelIv == null)
    //        {
    //            myRijndael.IV = GenerateXBytesOfRandomEntropy(16); //IV must be 16 bytes / 128 bit
    //            eO.RijndaelIv = myRijndael.IV;
    //        }
    //        else
    //        {
    //            myRijndael.IV = eO.RijndaelIv;
    //        }

    //        myRijndael.Padding = PaddingMode.PKCS7;
    //        myRijndael.Mode = CipherMode.CBC;

    //        // Using Scrypt for Key Derivation
    //        if (eO.DerivationType == null || eO.DerivationType == "scrypt")
    //        {
    //            throw new Exception("Only DerivationType rfc is supported");
    //            //eO.DerivationType = "scrypt";
    //            //myRijndael.Key =
    //            //    ScryptHandler.GetOnlyHashBytes(System.Text.Encoding.UTF8.GetBytes(passPhrase), eO);
    //        }
    //        // Using RFC2898 for Key Derivation
    //        else
    //        {
    //            if (eO.Salt == null)
    //            {
    //                eO.Salt = GenerateXBytesOfRandomEntropy(32);
    //            }
    //            myRijndael.KeySize = eO.KeySizeInBytes * 8;
    //            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(System.Text.Encoding.UTF8.GetBytes(passPhrase), eO.Salt,
    //                eO.DerivationIterations/*, HashAlgorithmName.SHA256*/);
    //            myRijndael.Key = rfc2898.GetBytes(eO.KeySizeInBytes);
    //        }

    //        byte[] utf8Text = bytesToEncrypt;
    //        ICryptoTransform transform = myRijndael.CreateEncryptor();
    //        byte[] cipherText = transform.TransformFinalBlock(utf8Text, 0, utf8Text.Length);
    //        var cipherWithSaltAndIvObject = new CipherResult(eO, cipherText);
    //        return cipherWithSaltAndIvObject;
    //    }
    //    public static string DecryptFromJson(string cipherTextJson, string passPhrase)
    //    {
    //        //CipherResultText cO = JsonConvert.DeserializeObject<CipherResultText>(cipherTextJson);
    //        CipherResultText cO = JsonSerializer.Deserialize<CipherResultText>(cipherTextJson);

    //        var cipherObject = cO.ConvertToCipherObject();
    //        var plainTextAsBytes = BasicAesDecryption(cipherObject, passPhrase);
    //        return System.Text.Encoding.UTF8.GetString(plainTextAsBytes);
    //    }
    //    public static string Decrypt(string cipherText, string passPhrase)
    //    {
    //        EncryptionOptions options = new EncryptionOptions();
    //        var parts = cipherText.Split("|");
    //        CipherResultText cO = new CipherResultText
    //        {
    //            AesRijndaelIv = parts[0],
    //            BlockSize = options.BlockSize,
    //            Cost = options.Cost,
    //            Parallel = options.Parallel,
    //            KeySizeInBytes = options.KeySizeInBytes,
    //            DerivationIterations = options.DerivationIterations,
    //            DerivationType = options.DerivationType,
    //            Salt = parts[2],
    //            CipherOutputText = parts[1]
    //        };
    //        var cipherObject = cO.ConvertToCipherObject();
    //        var plainTextAsBytes = BasicAesDecryption(cipherObject, passPhrase);
    //        return System.Text.Encoding.UTF8.GetString(plainTextAsBytes);
    //    }
    //    // The CipherResult contains all important settings (options) and the resulting CipherText (in Byte Array)
    //    public static byte[] BasicAesDecryption(CipherResult cO, string passPhrase)
    //    {
    //        var myRijndael = new RijndaelManaged();
    //        myRijndael.BlockSize = 128;
    //        myRijndael.KeySize = cO.KeySizeInBytes * 8;
    //        myRijndael.IV = cO.AesRijndaelIv;
    //        myRijndael.Padding = PaddingMode.PKCS7;
    //        myRijndael.Mode = CipherMode.CBC;
    //        var salt = cO.Salt;
    //        if (cO.DerivationType == "scrypt")
    //        {
    //            throw new Exception("Only DerivationType rfc is supported");
    //            //myRijndael.Key =
    //            //    ScryptHandler.GetOnlyHashBytes(System.Text.Encoding.UTF8.GetBytes(passPhrase), cO);
    //        }
    //        else
    //        {
    //            Rfc2898DeriveBytes rfc2898 = new Rfc2898DeriveBytes(System.Text.Encoding.UTF8.GetBytes(passPhrase), salt,
    //                cO.DerivationIterations);
    //            myRijndael.Key = rfc2898.GetBytes(cO.KeySizeInBytes);
    //        }

    //        var encryptedBytes = cO.CipherOutput;
    //        ICryptoTransform transform = myRijndael.CreateDecryptor();
    //        byte[] cipherText = transform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
    //        return cipherText;

    //    }

    //    public static byte[] GenerateXBytesOfRandomEntropy(int x)
    //    {
    //        var randomBytes = new byte[x]; // 32 Bytes will give us 256 bits.
    //        using (var rngCsp = new RNGCryptoServiceProvider())
    //        {
    //            // Fill the array with cryptographically secure random bytes.
    //            rngCsp.GetBytes(randomBytes);
    //        }
    //        return randomBytes;
    //    }
    //}
}
