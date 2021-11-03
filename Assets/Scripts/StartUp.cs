using UnityEngine;

/// <summary>
/// 启动脚本
/// </summary>
public class StartUp : MonoBehaviour
{
    void Awake()
    {
        // 热更新之前初始化一些模块
        InitBeforeHotUpdate();
        // 热更新
        HotUpdatePanel.Create(() =>
        {
            // 热更新后初始化一些模块
            InitAfterHotUpdate();
            // 启动游戏
            StartGame();
        });
    }

    private void StartGame()
    {
        //启动lua框架//
        LuaFramework.AppFacade.Instance.StartUp(() =>
        {
            LuaCall.CallFunc("Game.Init");

            LuaCall.CallFunc("Game.Start");

            // 测试
            Test.Create();
        });
    }

    /// <summary>
    /// 热更新之前初始化一些模块
    /// </summary>
    private void InitBeforeHotUpdate()
    {
        m_networkMsgEventRegister = new NetworkMsgEventRegister();
        //手机常亮//
        Screen.sleepTimeout = -1;
        //后台运行//
        Application.runInBackground = true;

        Logger.instance.Init();
        LogCat.Init();
        // 网络消息注册
        m_networkMsgEventRegister.RegistNetworkMsgEvent();
        // 界面管理器
        PanelMgr.instance.Init();

        // 版本号
        VersionMgr.instance.Init();

        // 预加载AssetBundle
        AssetBundleMgr.instance.PreloadAssetBundles();
        // TODO 加载必要的资源AssetBundle


        TimerThread.instance.Init();
        ClientNet.instance.Init();
        ScreenCapture.Init();
    }

    /// <summary>
    /// 热更新后初始化一些模块
    /// </summary>
    private void InitAfterHotUpdate()
    {
        ResourceManager.instance.Init();
        AudioMgr.instance.Init();
        I18N.instance.Init();
        SpriteManager.instance.Init();
    }

    private void Update()
    {
        LuaFramework.AppFacade.Instance.UpdateEx();
    }

    private void LateUpdate()
    {
        LuaFramework.AppFacade.Instance.LateUpdateEx();
    }

    private void FixedUpdate()
    {
        LuaFramework.AppFacade.Instance.FixedUpdateEx();
    }


    private NetworkMsgEventRegister m_networkMsgEventRegister;
}
