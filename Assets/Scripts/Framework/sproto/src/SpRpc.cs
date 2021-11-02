using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum SpRpcOp
{
    Request,
    Response,
    Unknown,
}

public class SpRpcResult
{
    public int Session;
    public SpProtocol Protocol;
    public SpRpcOp Op;
    public SpObject Arg;
    
    // 是否已经处理了
    public bool bProcess = false;

    public SpRpcResult()
    {
        Session = 0;
        Protocol = null;
        Op = SpRpcOp.Unknown;
        Arg = null;
    }

    public SpRpcResult(int s, SpProtocol p, SpRpcOp o, SpObject a)
    {
        Session = s;
        Protocol = p;
        Op = o;
        Arg = a;
    }
}

public class SpRpc
{
    private SpTypeManager mHostTypeManager;
    private SpTypeManager mAttachTypeManager;

    private SpType mHeaderType;
    private Dictionary<int, SpProtocol> mSessions = new Dictionary<int, SpProtocol>();

    public SpTypeManager hostTypeManager { get { return mHostTypeManager; } }
    public SpTypeManager attachTypeManager { get { return mAttachTypeManager; } }

    public SpRpc(SpTypeManager tm, SpType t)
    {
        mHostTypeManager = tm;
        mHeaderType = t;
    }

    public void Attach(string proto)
    {
        Attach(SpTypeManager.Import(proto));
    }

    public void Attach(SpTypeManager tm)
    {
        mAttachTypeManager = tm;
    }

    public SpStream Request(string proto)
    {
        return Request(proto, null);
    }

    public SpStream Request(string proto, SpObject args)
    {
        return Request(proto, args, 0);
    }

    public SpStream Request(string proto, SpObject args, int session)
    {
        SpStream encode_stream = EncodeRequest(proto, args, session);
        encode_stream.Position = 0;
        return SpPacker.Pack(encode_stream);
    }

    public bool Request(string proto, SpObject args, int session, SpStream stream)
    {
        SpStream encode_stream = EncodeRequest(proto, args, session);
        encode_stream.Position = 0;
        return SpPacker.Pack(encode_stream, stream);
    }

    private SpStream EncodeRequest(string proto, SpObject args, int session)
    {
        if (mAttachTypeManager == null || mHostTypeManager == null || mHeaderType == null)
            return null;

        SpProtocol protocol = mAttachTypeManager.GetProtocolByName(proto);
        if (protocol == null)
            return null;

        SpObject header = new SpObject();
        header.Insert("type", protocol.Tag);
        if (session != 0)
            header.Insert("session", session);

        SpStream stream = mHostTypeManager.Codec.Encode(mHeaderType, header);
        if (stream == null)
            return null;

        if (args != null)
        {
            if (mAttachTypeManager.Codec.Encode(protocol.Request, args, stream) == false)
            {
                if (stream.IsOverflow())
                {
                    if (stream.Position > SpCodec.MAX_SIZE)
                        return null;

                    int size = stream.Position;
                    size = ((size + 7) / 8) * 8;
                    stream = new SpStream(size);
                    if (mAttachTypeManager.Codec.Encode(protocol.Request, args, stream) == false)
                        return null;
                }
                else
                {
                    return null;
                }
            }
        }

        if (session != 0)
        {
            mSessions[session] = protocol;
        }

        return stream;
    }

    public SpStream Response(int session, SpObject args)
    {
        SpObject header = new SpObject();
        header.Insert("session", session);

        SpStream encode_stream = new SpStream();
        mHostTypeManager.Codec.Encode(mHeaderType, header, encode_stream);

        if (session != 0 && mSessions.ContainsKey(session))
        {
            mHostTypeManager.Codec.Encode(mSessions[session].Response, args, encode_stream);
        }

        SpStream pack_stream = new SpStream();
        encode_stream.Position = 0;
        SpPacker.Pack(encode_stream, pack_stream);

        pack_stream.Position = 0;
        return pack_stream;
    }

    public SpRpcResult Dispatch(SpStream stream)
    {
        SpStream unpack_stream = SpPacker.Unpack(stream);

        unpack_stream.Position = 0;
        SpObject header = mHostTypeManager.Codec.Decode(mHeaderType, unpack_stream);

        int session = 0;
        if (header["session"] != null)
        {
            session = header["session"].AsInt();
        }

        if (header["type"] != null)
        {
            int type = header["type"].AsInt();

            bool bProcess = LuaFramework.NetworkManager.OnRequestData(unpack_stream, type);
            if (!bProcess)
            {
                //lua单独处理
                return null;
            }
            // handle request
            SpProtocol protocol = mHostTypeManager.GetProtocolByTag(type);
            if (protocol == null)
            {
                // 找不到服务器推送的消息协议，返回
                GameLogger.LogError("not find sproto message id, " + type);
                SpStreamCache.Collect(unpack_stream);
                return null;
            }

            SpObject obj = mHostTypeManager.Codec.Decode(protocol.Request, unpack_stream);
            if (session != 0)
                mSessions[session] = protocol;
            SpStreamCache.Collect(unpack_stream);
            return new SpRpcResult(session, protocol, SpRpcOp.Request, obj);
        }
        else
        {
            bool bProcess = true;
            SpProtocol protocol = null;
            if (mSessions.TryGetValue(session, out protocol))
            {
                bProcess = LuaFramework.NetworkManager.OnResponseData(unpack_stream, protocol, session);
            }
            else
            {
                bProcess = LuaFramework.NetworkManager.OnResponseData(unpack_stream, protocol, session);
            }

            mSessions.Remove(session);

            if (!bProcess)
            {
                //lua单独处理
                //return null;
                SpRpcResult result = new SpRpcResult(session, protocol, SpRpcOp.Response, null);
                result.bProcess = true;
                return result;
            }
            // handle response
            if (protocol == null)
            {
                GameLogger.LogError("not contain session: " + session);
                //return new SpRpcResult();
                return null;
            }

            //SpProtocol protocol = mSessions[session];


            if (protocol == null)
                return new SpRpcResult();

            if (protocol.Response == null)
                return new SpRpcResult(session, protocol, SpRpcOp.Response, null);

            SpObject obj = mAttachTypeManager.Codec.Decode(protocol.Response, unpack_stream);
            SpStreamCache.Collect(unpack_stream);
            return new SpRpcResult(session, protocol, SpRpcOp.Response, obj);
        }

    }

    //处理一些lua中发送的协议
    public void AddSession(int session, SpProtocol protocol, int tag)
    {
        if (session != 0)
        {
            if (protocol == null)
            {
                //mAttachTypeManager  (c2s)
                protocol = mAttachTypeManager.GetProtocolByTag(tag);
                //s2c
                //protocol = mHostTypeManager.GetProtocolByTag(tag);
            }
            mSessions[session] = protocol;
        }
    }


    public static SpRpc Create(Stream proto, string package)
    {
        return Create(SpTypeManager.Import(proto), package);
    }

    public static SpRpc Create(string proto, string package)
    {
        return Create(SpTypeManager.Import(proto), package);
    }

    public static SpRpc Create(SpTypeManager tm, string package)
    {
        if (tm == null)
            return null;

        SpType t = tm.GetType(package);
        if (t == null)
            return null;

        SpRpc rpc = new SpRpc(tm, t);
        return rpc;
    }
}
