
/// <summary>
/// 网络状态
/// </summary>
public enum NetState
{
    ConnectStart,
    ConnectSuccess,
    ConnectFail,
    Disconnect,
    ResendStart,
    ResendSuccess,
    ResendFail,
    Error
}

public interface INetStateListener
{
    void OnNetStateChanged(NetState state, object param = null);
}

