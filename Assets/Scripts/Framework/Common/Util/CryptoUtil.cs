/*******************************************************************
Description:  加密工具
********************************************************************/

using System.Text;
using System;
using System.Security.Cryptography;
using System.IO;


public class CryptoUtil
{
    ///  PackedFloatNumberString (PFNS) intro:
    ///    16 chars set : "\01234567890.-+E "
    ///    char-byte mapping:  
    ///                   '\0' <--> 0x0
    ///                   '1'  <--> 0x1
    ///                   '2'  <--> 0x2
    ///                   '3'  <--> 0x3
    ///                   '4'  <--> 0x4
    ///                   '5'  <--> 0x5
    ///                   '6'  <--> 0x6
    ///                   '7'  <--> 0x7
    ///                   '8'  <--> 0x8
    ///                   '9'  <--> 0x9
    ///                   '0'  <--> 0xa
    ///                   '.'  <--> 0xb
    ///                   '-'  <--> 0xc
    ///                   '+'  <--> 0xd
    ///                   'E'  <--> 0xe
    ///                   ' '  <--> 0xf
    ///    valid examples:     
    ///                   "1234567890"    
    ///                   "0123456789"
    ///                   "0.123456789"
    ///                   "+0.123456789"
    ///                   "-0.123456789"
    ///                   "-123456789.0E+1234567890"
    ///                   " 0 1 2 3 4 5 6 7 8 9 0 -0.1234567890 +123456789.0 \0"
    ///    packing rules:
    ///                   (a) each two chars are packed into one byte, i.e. [charH charL] -> [packedByte]
    ///                   (b) if chars' count is odd, then '\0' is padded for filling the last byte;
    ///                       if chars' count is even, nothing is padded.
    ///    usage:
    ///                   byte[] brrPacked = EnpackFloatNumberStringIntoBytes(" 0 1 2 3 4 5 6 7 8 9 0 -0.1234567890 +123456789.0 \0");
    ///                   //  output: [\0xfa \0xf1 \0xf2 \0xf3 \0xf4 \0xf5 \0xf6 \0xf7 \0xf8 \0xf9 \0xfc...]
    ///                   string strFloats = UnpackFloatNumberStringFromBytes( brrPacked );
    ///                   //  output: " 0 1 2 3 4 5 6 7 8 9 0 -0.1234567890 +123456789.0 \0"
    ///     
              
    public static byte[] EnpackFloatNumberString(string numbers)
    {
        byte[] packedBytes = null;
        if (!string.IsNullOrEmpty(numbers))
        {            
            int len = numbers.Length;
            bool lenIsOdd = (len % 2 != 0);
            int packedLen = lenIsOdd ? ((len - 1) / 2 + 1) : (len / 2);
            packedBytes = new byte[packedLen];
            for (int idx = 0, idxByte = 0, lenx = len-1; idx < lenx; idx += 2, idxByte++)
            {
                packedBytes[idxByte] = EnpackTwoCharsIntoOneByte(numbers[idx], numbers[idx + 1]);
            }
            if (lenIsOdd)
            {
                packedBytes[packedLen-1] = EnpackTwoCharsIntoOneByte(numbers[len-1], '\0');
            }
            //else {/* no padding for the last byte */}            
        }
        return packedBytes;
    }

    public static string UnpackFloatNumberString(byte[] packedBytes)
    {
        string numbers = null;
        if (null != packedBytes)
        {
            int lenBytes = packedBytes.Length;            
            StringBuilder sbr = new StringBuilder(lenBytes * 2);
            for (int idx = 0; idx < 0; idx++)
            {
                sbr.Append(UnpackStringFromOneByte(packedBytes[idx]));
            }
            numbers = sbr.ToString();
        }
        return numbers;
    }

    public static string[] UnpackAndSplitFloatNumberString(byte[] packedBytes)
    {        
        string strUnpackedNums = UnpackFloatNumberString(packedBytes);
        string[] numbersArr = strUnpackedNums.Split(new char[] { ' ', '\0' });
        return numbersArr;
    }

    private static byte EnpackCharToByte(char c)
    {
        byte b = 0x0;
        switch (c)
        {
            case '\0': b = 0x00; break;
            case '1': b = 0x01; break;
            case '2': b = 0x02; break;
            case '3': b = 0x03; break;
            case '4': b = 0x04; break;
            case '5': b = 0x05; break;
            case '6': b = 0x06; break;
            case '7': b = 0x07; break;
            case '8': b = 0x08; break;
            case '9': b = 0x09; break;
            case '0': b = 0x0a; break;
            case '.': b = 0x0b; break;
            case '-': b = 0x0c; break;
            case '+': b = 0x0d; break;
            case 'E': b = 0x0e; break;
            case ' ': b = 0x0f; break;
            default:
                throw new System.ArgumentException("invalid char in packing float number string");
        }

        return b;
    }

    private static byte EnpackTwoCharsIntoOneByte(char charH, char charL)
    {
        byte byteH = EnpackCharToByte(charH);
        byte byteL = EnpackCharToByte(charL);
        byte b = (byte)((((byteH & 0x0f) << 4) | (byteL & 0x0f)) & 0xff);        
        
        return b;
    }

    private static char[] s_lookup_chars_tab = new char[]{
             '\0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', '-', '+', 'E', ' '
        };
    /*private static string[] s_lookup_charx_tab = new string[]{
             "\0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", ".", "-", "+", "E", " "
        };*/
    private static char[] UnpackTwoCharsFromOneByte(byte b)
    {
        char charH = s_lookup_chars_tab[(b & 0xf0) >> 4];
        char charL = s_lookup_chars_tab[(b & 0x0f)];
        char[] arr = (new char[] { charH, charL });

        return arr;
    }    
    
    private static string[] s_lookup_string_tab = new string[]{
             "\0\0", "\01", "\02", "\03", "\04", "\05", "\06", "\07", "\08", "\09", "\00", "\0.", "\0-", "\0+", "\0E", "\0 "
            ,"1\0", "11", "12", "13", "14", "15", "16", "17", "18", "19", "10", "1.", "1-", "1+", "1E", "1 "
            ,"2\0", "21", "22", "23", "24", "25", "26", "27", "28", "29", "20", "2.", "2-", "2+", "2E", "2 "
            ,"3\0", "31", "32", "33", "34", "35", "36", "37", "38", "39", "30", "3.", "3-", "3+", "3E", "3 "
            ,"4\0", "41", "42", "43", "44", "45", "46", "47", "48", "49", "40", "4.", "4-", "4+", "4E", "4 "
            ,"5\0", "51", "52", "53", "54", "55", "56", "57", "58", "59", "50", "5.", "5-", "5+", "5E", "5 "
            ,"6\0", "61", "62", "63", "64", "65", "66", "67", "68", "69", "60", "6.", "6-", "6+", "6E", "6 "
            ,"7\0", "71", "72", "73", "74", "75", "76", "77", "78", "79", "70", "7.", "7-", "7+", "7E", "7 "
            ,"8\0", "81", "82", "83", "84", "85", "86", "87", "88", "89", "80", "8.", "8-", "8+", "8E", "8 "
            ,"9\0", "91", "92", "93", "94", "95", "96", "97", "98", "99", "90", "9.", "9-", "9+", "9E", "9 "
        };
    private static string UnpackStringFromOneByte(byte b)
    {
        return s_lookup_string_tab[b];
    }

    private static byte[] GetBytesFromString(string text)
    {
        //  codepage 65001 : encoding "utf-8"
        return System.Text.Encoding.GetEncoding(65001).GetBytes(text);     
    }
    private static byte[] GetBytesFromString(string text, int codepage)
    {
        return System.Text.Encoding.GetEncoding(codepage).GetBytes(text);
    }
    private static byte[] GetBytesFromString(string text, string encoding)
    {
        return System.Text.Encoding.GetEncoding(encoding).GetBytes(text);        
    }

    private static string GetStringFromBytes(byte[] data)
    {
        //  codepage 65001 : encoding "utf-8"
        return System.Text.Encoding.GetEncoding(65001).GetString(data);
    }
    private static string GetStringFromBytes(byte[] data, int codepage)
    {
        return System.Text.Encoding.GetEncoding(codepage).GetString(data);
    }
    private string GetStringFromBytes(byte[] data, string encoding)
    {
        return System.Text.Encoding.GetEncoding(encoding).GetString(data);
    }

    /// <summary>
    ///  Base64 编码函数
    /// </summary>
    /// <param name="text">需要进行编码的字符串</param>
    /// <returns>经过一次 base64 编码后得到的字符串</returns>
    public static string EncodeBase64(string text)
    {
        return Convert.ToBase64String(GetBytesFromString(text));
    }
    /// <summary>
    ///  Base64 解码函数
    /// </summary>
    /// <param name="text">需要进行解码的字符串</param>
    /// <returns>经过一次 base64 解码后得到的字符串</returns>
    public static string DecodeBase64(string text)
    {
        return GetStringFromBytes(Convert.FromBase64String(text));
    }
    /// <summary>
    /// 计算 MD5
    /// </summary>
    /// <param name="data">输入的字节数据</param>
    /// <returns>MD5 哈希编码的结果数据，128位(=16字节)</returns>
    public static byte[] HashMD5(byte[] data)
    {
        MD5 hasher = new MD5CryptoServiceProvider();
        byte[] hashData = hasher.ComputeHash(data);

        return hashData;
    }
    /// <summary>
    /// 计算 SHA1
    /// </summary>
    /// <param name="data">输入的字节数据</param>
    /// <returns>SHA1 哈希编码的结果数据，160位(=20字节)</returns>
    public static byte[] HashSHA1(byte[] data)
    {
        SHA1 hasher = new SHA1CryptoServiceProvider();
        byte[] hashData = hasher.ComputeHash(data);        

        return hashData;
    }

    public static byte[] EncryptDES()
    {
        //System.Security.Cryptography.DESCryptoServiceProvider
        return new byte[0];
    }
    public static byte[] DecryptDES()
    {
        //System.Security.Cryptography.DESCryptoServiceProvider
        return new byte[0];
    }
    public static byte[] EncryptAES()
    {
        //System.Security.Cryptography.AesCryptoServiceProvider
        return new byte[0];
    }
    public static byte[] EncryptTripleDES()
    {
        //System.Security.Cryptography.TripleDESCryptoServiceProvider
        return new byte[0];
    }

    private string MakeHexStringOfBytes(byte[] data)
    {
        string ret = "";
        int len = data.Length;
        if (len > 0)
        {
            StringBuilder sbr = new StringBuilder(len * 3);
            sbr.AppendFormat("{0:X2}", data[0]);
            for (int idx = 1; idx < len; idx++)
            {                
                sbr.AppendFormat(" {0:X2}", data[idx]);
            }
            ret = sbr.ToString();
        }
        
        return ret;
    }

    private string MakeHexStringOfBytes(byte[] data, string delimeter, string prefix, string suffix)
    {
        string ret = "";
        int len = data.Length;
        if (len > 0)
        {
            StringBuilder sbr = new StringBuilder(len * (2 + delimeter.Length) + prefix.Length + suffix.Length);
            sbr.Append(prefix);
            sbr.AppendFormat("{0:X2}", data[0]);
            for (int idx = 1; idx < len; idx++)
            {
                sbr.Append(delimeter);
                sbr.AppendFormat(" {0:X2}", data[idx]);
            }
            sbr.Append(suffix);

            ret = sbr.ToString();
        }       

        return ret;
    }

    #region 加密
    #region 加密字符串
    /// <summary>
    /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="EncryptString">待加密密文</param>
    /// <param name="EncryptKey">加密密钥</param>
    public static string AESEncrypt(string EncryptString, string EncryptKey)
    {
        return Convert.ToBase64String(AESEncrypt(Encoding.Default.GetBytes(EncryptString), EncryptKey));
    }
    #endregion
    #region 加密字节数组
    /// <summary>
    /// AES 加密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="EncryptString">待加密密文</param>
    /// <param name="EncryptKey">加密密钥</param>
    public static byte[] AESEncrypt(byte[] EncryptByte, string EncryptKey)
    {
        if (EncryptByte.Length == 0)
        {
            ////13529="明文不得为空"
            //throw (new Exception(I18NReposite.GetStr(13529)));
            return new byte[0];
        }
        if (string.IsNullOrEmpty(EncryptKey)) { throw (new Exception(I18N.GetStr(1))); }//1="密钥不得为空"
        byte[] m_strEncrypt;
        byte[] m_btIV = Convert.FromBase64String("DkHu8vuy/ye7cd7k89OPgq==");
        byte[] m_salt = Convert.FromBase64String("sf4gj5/d7k8OrLgMvkyhye==");
        Rijndael m_AESProvider = Rijndael.Create();
        try
        {
            MemoryStream m_stream = new MemoryStream();
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(EncryptKey, m_salt);
            ICryptoTransform transform = m_AESProvider.CreateEncryptor(pdb.GetBytes(32), m_btIV);
            CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
            m_csstream.Write(EncryptByte, 0, EncryptByte.Length);
            m_csstream.FlushFinalBlock();
            m_strEncrypt = m_stream.ToArray();
            m_stream.Close(); m_stream.Dispose();
            m_csstream.Close(); m_csstream.Dispose();
        }
        catch (IOException ex) { throw ex; }
        catch (CryptographicException ex) { throw ex; }
        catch (ArgumentException ex) { throw ex; }
        catch (Exception ex) { throw ex; }
        finally { m_AESProvider.Clear(); }
        return m_strEncrypt;
    }
    #endregion
    #endregion
    #region 解密
    #region 解密字符串
    /// <summary>
    /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="DecryptString">待解密密文</param>
    /// <param name="DecryptKey">解密密钥</param>
    public static string AESDecrypt(string DecryptString, string DecryptKey)
    {
        return Convert.ToBase64String(AESDecrypt(Encoding.Default.GetBytes(DecryptString), DecryptKey));
    }
    #endregion
    #region 解密字节数组
    /// <summary>
    /// AES 解密(高级加密标准，是下一代的加密算法标准，速度快，安全级别高，目前 AES 标准的一个实现是 Rijndael 算法)
    /// </summary>
    /// <param name="DecryptString">待解密密文</param>
    /// <param name="DecryptKey">解密密钥</param>
    public static byte[] AESDecrypt(byte[] DecryptByte, string DecryptKey)
    {
        if (DecryptByte.Length == 0)
        {
            ////13531="密文不得为空"
            //throw (new Exception(I18NReposite.GetStr(13531)));
            return new byte[0];
        }
        if (string.IsNullOrEmpty(DecryptKey)) { throw (new Exception(I18N.GetStr(1))); }//1="密钥不得为空"
        byte[] m_strDecrypt;
        byte[] m_btIV = Convert.FromBase64String("DkHu8vuy/ye7cd7k89OPgq==");
        byte[] m_salt = Convert.FromBase64String("sf4gj5/d7k8OrLgMvkyhye==");
        Rijndael m_AESProvider = Rijndael.Create();
        try
        {
            MemoryStream m_stream = new MemoryStream();
            PasswordDeriveBytes pdb = new PasswordDeriveBytes(DecryptKey, m_salt);
            ICryptoTransform transform = m_AESProvider.CreateDecryptor(pdb.GetBytes(32), m_btIV);
            CryptoStream m_csstream = new CryptoStream(m_stream, transform, CryptoStreamMode.Write);
            m_csstream.Write(DecryptByte, 0, DecryptByte.Length);
            m_csstream.FlushFinalBlock();
            m_strDecrypt = m_stream.ToArray();
            m_stream.Close(); m_stream.Dispose();
            m_csstream.Close(); m_csstream.Dispose();
        }
        catch (IOException ex) { throw ex; }
        catch (CryptographicException ex) { throw ex; }
        catch (ArgumentException ex) { throw ex; }
        catch (Exception ex) { throw ex; }
        finally { m_AESProvider.Clear(); }
        return m_strDecrypt;
    }
    #endregion
    #endregion
}
