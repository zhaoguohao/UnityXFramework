using System.Collections.Generic;
using System.Text;

/// <summary>
/// 网络分析类
/// </summary>
public class NetDataAnalysis
{
    private static NetDataAnalysis _instance;
    public static NetDataAnalysis GetInstance()
    {
        lock (typeof(NetDataAnalysis))
        {
            if (_instance == null)
            {
                _instance = new NetDataAnalysis();
            }

            return _instance;
        }
    }

    private Dictionary<string, int> m_sendMsgDic = new Dictionary<string, int>();
    private Dictionary<string, int> m_receiveMsgDic = new Dictionary<string, int>();
    private StringBuilder m_stringBuilder = new StringBuilder();

    protected void AddSendData(string proto, int size)
    {
        int count;
        if(m_sendMsgDic.TryGetValue(proto, out count))
        {
            m_sendMsgDic[proto] = count + size;
        }
        else
        {
            m_sendMsgDic.Add(proto, size);
        }
    }

    protected void AddReceiveData(string proto, int size)
    {
        int count;
        if (m_receiveMsgDic.TryGetValue(proto, out count))
        {
            m_receiveMsgDic[proto] = count + size;
        }
        else
        {
            m_receiveMsgDic.Add(proto, size);
        }
    }


    public string GetDeBugInfo()
    {
        if (m_stringBuilder.Length > 0)
        {
            m_stringBuilder.Remove(0, m_stringBuilder.Length - 1);
        }
       

        int totalSendSize = 0;
        
        foreach (KeyValuePair<string, int> kvp in m_sendMsgDic)
        {
            totalSendSize += kvp.Value;
            m_stringBuilder.AppendFormat("Proto:{0},Size:{1}Byte/{2:N3}KB/{3:N6}M    ", kvp.Key, kvp.Value, kvp.Value / 1024.0f, kvp.Value / 1024.0f / 1024.0f);
        }
        
        m_stringBuilder.Append("\n");
        m_stringBuilder.AppendFormat(I18N.GetStr(2), totalSendSize, totalSendSize / 1024.0f, totalSendSize / 1024.0f / 1024.0f);//2="发送流量: {0}Byte/{1:N3}KB/{2:N6}M \n"

        int totalReceiveSize = 0;
        
        foreach (KeyValuePair<string, int> kvp in m_receiveMsgDic)
        {
            totalReceiveSize += kvp.Value;
            m_stringBuilder.AppendFormat("Proto:{0},Size:{1}Byte/{2:N3}KB/{3:N6}M    ", kvp.Key, kvp.Value, kvp.Value / 1024.0f, kvp.Value / 1024.0f / 1024.0f);
        }
        m_stringBuilder.Append("\n");
        m_stringBuilder.AppendFormat(I18N.GetStr(3), totalReceiveSize, totalReceiveSize / 1024.0f, totalReceiveSize / 1024.0f / 1024.0f);//3="接受总流量: {0}Byte/{1:N3}KB/{2:N6}M \n"

        return m_stringBuilder.ToString();
    }



    private static bool m_openAnalysis = false;

    public static void SetOpenNetAnalysis(bool bOpen)
    {
        m_openAnalysis = bOpen;
    }
    public static bool GetOpenAnalysis()
    {
        return m_openAnalysis;
    }

    public static void SendData(string proto, byte[] data)
    {
        if(m_openAnalysis)
        {
            NetDataAnalysis.GetInstance().AddSendData(proto, data.Length);
        }
    }

    public static void RecieveData(SpRpcResult spResult, SpStream spStream)
    {
        if(m_openAnalysis)
        {
            NetDataAnalysis.GetInstance().AddReceiveData(spResult.Protocol.Name, spStream.Length);
        }
    }

}
