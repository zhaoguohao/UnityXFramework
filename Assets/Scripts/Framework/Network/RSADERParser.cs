using System;
using System.IO;
using System.Security.Cryptography;

/// <summary>
/// RSADER加密解密
/// </summary>
public class RSADERParser
{

    public struct DERNotation
    {
        public byte tag;
        public byte[] content;
        public DERNotation[] constructed;
    };

    static void DERDecode(BinaryReader binr, int len, ref DERNotation up)
    {
        long pos = binr.BaseStream.Position;
        while (binr.BaseStream.Position < pos + len)
        {
            byte tag = binr.ReadByte();
            int nlen = DERGetNLength(binr);
            DERNotation n = new DERNotation() { tag = tag };
            if ((tag & 0x20) == 0)
            {
                n.content = binr.ReadBytes(nlen);
            }
            else
            {
                n.constructed = new DERNotation[0];
                DERDecode(binr, nlen, ref n);
            }
            Array.Resize<DERNotation>(ref up.constructed, up.constructed.Length + 1);
            up.constructed.SetValue(n, up.constructed.Length - 1);
        }
    }

    static public DERNotation DERDecode(byte[] data, int offset)
    {
        using (BinaryReader binr = new BinaryReader(new MemoryStream(data)))
        {
            if (offset > 0)
            {
                binr.ReadBytes(offset);
            }
            byte tag = binr.ReadByte();
            int nlen = DERGetNLength(binr);
            DERNotation n = new DERNotation() { tag = tag };
            if ((tag & 0x20) == 0)
            {
                n.content = binr.ReadBytes(nlen);
                return n;
            }
            n.constructed = new DERNotation[0];
            DERDecode(binr, nlen, ref n);
            //UnityEngine.GameLogger.LogYellow(DERDump(n));
            return n;
        }
    }

    static public string DERDump(DERNotation n)
    {
        string s = "{";
        s += "tag:" + n.tag.ToString();
        if (n.content != null)
        {
            s += ",content:" + BytesToHexString(n.content);
        }
        else if (n.constructed != null)
        {
            s += ",constructed:[";
            for (int i = 0; i < n.constructed.Length; i++)
            {
                if (i > 0)
                    s += ",";
                s += DERDump(n.constructed[i]);
            }
            s += "]";
        }
        s += "}";
        return s;
    }

    static byte[] HexStringToBytes(string HexString)
    {
        if (HexString.Length % 2 != 0)
            HexString += " ";
        byte[] retBytes = new byte[HexString.Length / 2];
        for (int i = 0; i < retBytes.Length; i++)
        {
            retBytes[i] = Convert.ToByte(HexString.Substring(i * 2, 2), 16);
        }
        return retBytes;
    }

    static string BytesToHexString(byte[] Bytes)
    {
        string retStr = "";
        for (int i = 0; i < Bytes.Length; i++)
        {
            retStr += string.Format("{0:X2}", Bytes[i]);
        }
        return retStr;
    }

    static int DERGetNLength(BinaryReader binr)
    {
        byte bt = 0;
        byte lowbyte = 0x00;
        byte highbyte = 0x00;
        int count = 0;
        bt = binr.ReadByte();
        if (bt < 0x80)
            count = bt;
        else if (bt == 0x81)
            count = binr.ReadByte();
        else if (bt == 0x82)
        {
            highbyte = binr.ReadByte();
            lowbyte = binr.ReadByte();
            byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
            count = BitConverter.ToInt32(modint, 0);
        }
        else
            throw new Exception("DERGetNLength Unexpected length");
        return count;
    }

    /*
        PrivateKeyInfo ::= SEQUENCE {
            version INTEGER,
            privateKeyAlgorithm PrivateKeyAlgorithmIdentifier,
            privateKey OCTET_STRING -- PrivateKey DER code
        }
        PrivateKeyAlgorithmIdentifier ::= SEQUENCE {
            oid OBJECT_IDENTIFIER,
            TAG_NULL
        }
        PrivateKey ::= SEQUENCE {
            modulus INTEGER,
            exponent INTEGER,
            d INTEGER,
            p INTEGER,
            q INTEGER,
            dp INTEGER,
            dq INTEGER,
            inverseq INTEGER
        }
    */
    static public RSAParameters ParsePrivateKey(byte[] PrivKeyBytes)
    {
        RSAParameters RSAparams = new RSAParameters();

        DERNotation n = DERDecode(PrivKeyBytes, 0);
        n = DERDecode(n.constructed[2].content, 0);

        // n.constructed[0] is version
        RSAparams.Modulus = n.constructed[1].content;
        RSAparams.Exponent = n.constructed[2].content;
        RSAparams.D = n.constructed[3].content;
        RSAparams.P = n.constructed[4].content;
        RSAparams.Q = n.constructed[5].content;
        RSAparams.DP = n.constructed[6].content;
        RSAparams.DQ = n.constructed[7].content;
        RSAparams.InverseQ = n.constructed[8].content;

        return RSAparams;
    }

    static public RSAParameters ParsePrivateKey(string PrivKey)
    {
        return ParsePrivateKey(HexStringToBytes(PrivKey));
    }

    static public RSACryptoServiceProvider CreateDecoder(byte[] PrivKeyBytes)
    {
        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
        RSA.ImportParameters(ParsePrivateKey(PrivKeyBytes));
        return RSA;
    }

    static public RSACryptoServiceProvider CreateDecoder(string PrivKey)
    {
        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
        RSA.ImportParameters(ParsePrivateKey(PrivKey));
        return RSA;
    }

    /*
        PublicKeyInfo ::= SEQUENCE {
            privateKeyAlgorithm PrivateKeyAlgorithmIdentifier,
            publicKey BIT_STRING -- PublicKey DER code
        }
        PrivateKeyAlgorithmIdentifier ::= SEQUENCE {
            oid OBJECT_IDENTIFIER,
            TAG_NULL
        }
        PublicKey ::= SEQUENCE {
            modulus INTEGER,
            exponent INTEGER
        }
    */
    static public RSAParameters ParsePublicKey(byte[] PubKeyBytes)
    {
        RSAParameters RSAparams = new RSAParameters();

        DERNotation n = DERDecode(PubKeyBytes, 0);
        n = DERDecode(n.constructed[1].content, 1);

        RSAparams.Modulus = n.constructed[0].content;
        RSAparams.Exponent = n.constructed[1].content;

        return RSAparams;
    }

    static public RSAParameters ParsePublicKey(string PubKey)
    {
        return ParsePublicKey(HexStringToBytes(PubKey));
    }

    static public RSACryptoServiceProvider CreateEncoder(byte[] PubKeyBytes)
    {
        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
        RSA.ImportParameters(ParsePublicKey(PubKeyBytes));
        return RSA;
    }

    static public RSACryptoServiceProvider CreateEncoder(string PubKey)
    {
        RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
        RSA.ImportParameters(ParsePublicKey(PubKey));
        return RSA;
    }
}
