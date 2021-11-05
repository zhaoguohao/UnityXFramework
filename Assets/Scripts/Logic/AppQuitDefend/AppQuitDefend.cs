using UnityEngine;

/// <summary>
/// App退出时进行拦截，弹出提示框
/// </summary>
public class AppQuitDefend
{
    public static void Init()
    {
        Application.wantsToQuit += DontQuit;
    }

    public static bool DontQuit()
    {
        LuaCall.CallFunc("AppQuitLogic.Defend");
        return false;
    }

    public static void DoQuit()
    {
        Application.wantsToQuit -= DontQuit;
        Application.Quit();
    }
}
