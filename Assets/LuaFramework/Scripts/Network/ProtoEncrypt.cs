
using UnityEngine;
using System.Security.Cryptography;
using System.IO;

/// <summary>
/// 协议加密
/// </summary>
public class ProtoEncrypt
{
    private readonly byte[] kPublicKey = { 0xc8, 0x3d, 0x2d, 0x8d, 0x23, 0x2c, 0xdc, 0xd3, 0x3c, 0xc2, 0xc3, 0xc5, 0xd2, 0x8c, 0x3d, 0x88, 0x3c, 0xc3, 0x2d, 0x83, 0xd8, 0xc0, 0xdd, 0xdc, 0x35, 0x3c, 0xcc, 0x22, 0x8c, 0x85, 0xc5, 0x8d, 0x32, 0xd2, 0x85, 0x30, 0x02, 0x52, 0xc8, 0x5c, 0x50, 0xc0, 0x83, 0x8c, 0x53, 0x3c, 0x82, 0xc2, 0xd3, 0xd8, 0xcc, 0x35, 0xc5, 0x3d, 0x5c, 0xcc, 0x5d, 0x3c, 0x23, 0x3d, 0x20, 0x33, 0x5c, 0x30, 0xc8, 0x53, 0xc8, 0xd5, 0x5c, 0x03, 0xdd, 0x58, 0x32, 0x38, 0x5c, 0xc2, 0xd2, 0x53, 0xd2, 0xcc, 0x23, 0x88, 0x83, 0xc0, 0xcc, 0xcd, 0x8c, 0xc5, 0xd2, 0x03, 0xcc, 0xcd, 0x5d, 0x0c, 0x2c, 0x0c };

    private DESCryptoServiceProvider m_des = null;
    private ICryptoTransform m_des_encryptor = null;
    private ICryptoTransform m_des_decryptor = null;
    private byte[] m_des_key = null;

    public byte[] Init(byte[] rsa_public_key = null)
    {
        if (rsa_public_key == null)
            rsa_public_key = kPublicKey;
        var pk = DecodePublicKey(rsa_public_key);
        var key = GenDesKey();
        SetupDes(key);
        var key_data = RsaEncrypt(pk, key);
        int key_data_len = key_data.Length;
        byte[] data = new byte[key_data_len + 2];
        data[0] = (byte)((key_data_len >> 8) & 0xff);
        data[1] = (byte)(key_data_len & 0xff);
        System.Array.Copy(key_data, 0, data, 2, key_data_len);
        return data;
    }

    public void SetupDes(byte[] key)
    {
        m_des_key = key;
        m_des = CreateDES();
        m_des_encryptor = m_des.CreateEncryptor();
        m_des_decryptor = m_des.CreateDecryptor();
    }

    private DESCryptoServiceProvider CreateDES()
    {
        var m_des = new DESCryptoServiceProvider();
        m_des.Key = m_des_key;
        m_des.IV = m_des_key;
        m_des.Mode = CipherMode.ECB;
        m_des.Padding = PaddingMode.None;
        return m_des;
    }

    static readonly byte[] kPandding = new byte[] { 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };

    public static int CalcPanddingDataLen(int dataLen)
    {
        int mod = dataLen % 8;
        return dataLen + (8 - mod);
    }

    public int Encrypt(byte[] data, int dataOffset, int dataLen, byte[] outBuff, int outBuffOffset)
    {
        int mg = CalcPanddingDataLen(dataLen);
        if (outBuff.Length - outBuffOffset < mg)
            return -1;
        if (m_des == null)
            m_des = CreateDES();
        if (m_des_encryptor == null)
            m_des_encryptor = m_des.CreateEncryptor();
        MemoryStream mem = new MemoryStream(outBuff);
        mem.Position += outBuffOffset;

        CryptoStream st = new CryptoStream(mem, m_des_encryptor, CryptoStreamMode.Write);
        //st.Clear();
        st.Write(data, dataOffset, dataLen);
        if (mg > dataLen)
            st.Write(kPandding, 0, mg - dataLen);
        st.FlushFinalBlock();
        return (int)(mem.Position - outBuffOffset);
    }

    byte[] m_outBuffer = new byte[1024];
    int m_outBuffer_Capacity = 1024;
    //int m_outBuffer_Length = 0;

    private bool CheckOutBufferCapacity(int needCapacity)
    {
        if (needCapacity <= m_outBuffer_Capacity)
            return true;

        m_outBuffer = new byte[needCapacity];
        m_outBuffer_Capacity = needCapacity;

        return true;
    }

    public int EncryptProtoBeforeSend(SpStream inputStream, out byte[] outBuff)
    {
        int dataLen = inputStream.Length - 2;
        int mg = CalcPanddingDataLen(dataLen);

        if (mg > dataLen)
            inputStream.Write(kPandding, 0, mg - dataLen);

        //m_outBuffer_Length = 0;
        CheckOutBufferCapacity(mg + 2);
        if (m_des == null)
            m_des = CreateDES();
        if (m_des_encryptor == null)
            m_des_encryptor = m_des.CreateEncryptor();

        int len = m_des_encryptor.TransformBlock(inputStream.Buffer, 2, mg, m_outBuffer, 2);
        //Assert(count == mg);
        m_outBuffer[0] = (byte)((len >> 8) & 0xff);
        m_outBuffer[1] = (byte)(len & 0xff);
        outBuff = m_outBuffer;
        return len + 2;
    }


    public byte[] Decrypt(byte[] data, int offset, int len)
    {
        var d = m_des.CreateDecryptor();
        MemoryStream mem = new MemoryStream();
        CryptoStream st = new CryptoStream(mem, d, CryptoStreamMode.Write);
        st.Write(data, offset, len);
        st.FlushFinalBlock();
        return mem.ToArray();
    }


    public SpStream DecryptAsSpStream(byte[] data, int offset, int len)
    {
        if (len % 8 != 0)
            throw new System.ArgumentException("proto Encrypt : DecryptAsSpStream : len % 8 != 0 len=" + len);
        if (data == null)
            throw new System.ArgumentNullException("proto Encrypt : DecryptAsSpStream : data == null");
        if (m_des_decryptor == null)
            m_des_decryptor = m_des.CreateDecryptor();
        byte[] buff = new byte[len];
        int dLen = m_des_decryptor.TransformBlock(data, offset, len, buff, 0);
        dLen = CalcDataLen(buff, dLen);
        return new SpStream(buff, 0, 0, dLen);
    }

    private byte[] GenDesKey()
    {
        const int kDesKeyLength = 8;
        byte[] key = new byte[kDesKeyLength];
        // 旧写法
        // Random.seed = (int)System.DateTime.Now.Ticks;
        // 新写法
        Random.InitState((int)System.DateTime.Now.Ticks);
        for (int i = 0; i < key.Length; i++)
        {
            key[i] = (byte)(Random.Range(0, 255));
        }
        return key;
    }

    private static byte[] RsaEncrypt(byte[] rsaKey, byte[] data)
    {
        RSACryptoServiceProvider rsa = RSADERParser.CreateEncoder(rsaKey);
        return rsa.Encrypt(data, false);
    }

    private static byte[] DecodePublicKey(byte[] data)
    {
        var des = new DESCryptoServiceProvider();
        var dk = new byte[] { (byte)'e', (byte)'9', (byte)'f', (byte)'e', (byte)'6', (byte)'a', (byte)'3', (byte)'2' };
        des.Key = dk;
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.None;
        var e = des.CreateDecryptor();
        var decode_data = e.TransformFinalBlock(data, 0, data.Length);
        int len = SCalcDataLen(decode_data);
        if (len < decode_data.Length)
        {
            byte[] r = new byte[len];
            System.Array.Copy(decode_data, r, len);
            return r;
        }
        return decode_data;
    }

    public static int CalcDataLen(byte[] data)
    {
        if (data.Length < 8)
            return -1;
        int s = data.Length - 8;
        if (s < 0) s = 0;
        for (int i = data.Length - 1; i >= s; i--)
        {
            if (data[i] == 0x00)
                continue;
            if (data[i] == 0x80)
                return i;
            return -1;
        }
        return -1;
    }

    public static int CalcDataLen(byte[] data, int dataLen)
    {
        if (dataLen < 8)
            return -1;
        int s = dataLen - 8;
        if (s < 0) s = 0;
        for (int i = dataLen - 1; i >= s; i--)
        {
            if (data[i] == 0x00)
                continue;
            if (data[i] == 0x80)
                return i;
            return -1;
        }
        return -1;
    }

    public static int SCalcDataLen(byte[] data)
    {
        int len = CalcDataLen(data);
        if (len == -1) return data.Length;
        return len;
    }
}

