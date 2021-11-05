using System.Text;
using System.Security.Cryptography;

/// <summary>
/// AES加密解密
/// </summary>
public class AESEncrypt 
{
    /// <summary>
    /// 默认密钥-密钥的长度必须是32
    /// </summary>
    private const string PUBLIC_KEY = "Hello_I_am_linxinfa.WelcomeUnity";

    /// <summary>
    /// 默认向量
    /// </summary>
    private const string IV = "abcdefghijklmnop";

    /// <summary>
    /// AES加密
    /// </summary>
    /// <param name="str">需要加密的字符串</param>
    /// <param name="key">32位密钥</param>
    /// <returns>加密后的字符串</returns>
    public static byte[] Encrypt(byte[] toEncryptArray)
    {
        byte[] keyArray = Encoding.UTF8.GetBytes(PUBLIC_KEY);
        var rijndael = new RijndaelManaged();
        rijndael.Key = keyArray;
        rijndael.Mode = CipherMode.ECB;
        rijndael.Padding = PaddingMode.PKCS7;
        rijndael.IV = Encoding.UTF8.GetBytes(IV);
        ICryptoTransform cTransform = rijndael.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
        return resultArray;
    }

    /// <summary>
    /// AES解密
    /// </summary>
    /// <param name="str">需要解密的字符串</param>
    /// <param name="key">32位密钥</param>
    /// <returns>解密后的字符串</returns>
    public static byte[] Decrypt(byte[] toDecryptArray)
    {
        byte[] keyArray = Encoding.UTF8.GetBytes(PUBLIC_KEY);

        var rijndael = new RijndaelManaged();
        rijndael.Key = keyArray;
        rijndael.Mode = CipherMode.ECB;
        rijndael.Padding = PaddingMode.PKCS7;
        rijndael.IV = Encoding.UTF8.GetBytes(IV);
        ICryptoTransform cTransform = rijndael.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toDecryptArray, 0, toDecryptArray.Length);
        return resultArray;
    }
}
