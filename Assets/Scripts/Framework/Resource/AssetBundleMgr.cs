

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;


public class AssetBundleMgr
{
    public AssetBundleMgr()
    {
        m_bundles = new Dictionary<string, AssetBundle>();
        m_assets = new Dictionary<string, Object>();

    }

    public void PreloadAssetBundles()
    {
        #if UNITY_EDITOR
            // Nothing
        #else
            m_normalCfgBundle = LoadAssetBundle("normal_cfg.bundle");
        #endif
    }

    public Object LoadAsset(string uri)
    {
        System.Type t = GetAssetType(uri);
        if (m_assets.ContainsKey(uri))
            return m_assets[uri];

        Object obj = null;
#if UNITY_EDITOR
        obj = UnityEditor.AssetDatabase.LoadAssetAtPath("Assets/GameRes/" + uri, t);
#else
        // resources.bytes的第一级目录为AssetBundle
        var abName = uri.Substring(0, uri.IndexOf("/")).ToLower() + ".bundle";
        var fname = Path.GetFileName(uri);
        AssetBundle ab = null;
        if (File.Exists(updatePath + "/" + fname))
        {
            // 热更的资源，是一个独立的AssetBundle文件，以fname为文件名
            ab = LoadAssetBundle(fname);
        }
        else
        {
            ab = LoadAssetBundle(abName);
        }
        if (null != ab)
        {
            var assetName = fname.Substring(0, fname.IndexOf("."));
            obj = ab.LoadAsset<Object>(assetName);
            GameLogger.LogGreen("Load Asset From AssetBundle: assetName: " + assetName);
        }
#endif

        if (null != obj)
        {
            m_assets[uri] = obj;
            
        }
        return obj;
    }


    public AssetBundle LoadAssetBundle(string abName)
    {
        if (m_bundles.ContainsKey(abName))
            return m_bundles[abName];


        AssetBundle bundle = null;
        if (File.Exists(updatePath + "/" + abName))
        {
            // 优先从update目录（热更新目录）中查找资源
            bundle = AssetBundle.LoadFromFile(updatePath + "/" + abName);
        }
        else if (File.Exists(extPath + "/" + abName))
        {
            // 从拓展包目录加载资源
            bundle = AssetBundle.LoadFromFile(extPath + "/" + abName);
        }
        else
        {
            bundle = AssetBundle.LoadFromFile(internalPath + "/" + abName);
        }

        /*
        // 如果对AssetBundle做了加密处理，则需要使用流式读取，进行解密后再通过AssetBundle.LoadFromMemory加载AssetBundle
        byte[] stream = null;
        stream = File.ReadAllBytes(path + "/" + abName);
        // TOOD 对stream做解密

        var bundle = AssetBundle.LoadFromMemory(stream); 
        */

        if (null != bundle)
        {
            m_bundles[abName] = bundle;
            GameLogger.Log("LoadAssetBundle Ok, abName: " + abName);
        }
        
        return bundle;
    }




    protected System.Type GetAssetType(string uri)
    {
        if (uri.EndsWith(".prefab"))
        {
            return typeof(GameObject);
        }
        else if (uri.EndsWith(".ogg") || uri.EndsWith(".wav"))
        {
            return typeof(AudioClip);
        }
        else if(uri.EndsWith(".spriteatlas"))
        {
            return typeof(UnityEngine.U2D.SpriteAtlas);
        }
        else if (uri.EndsWith(".mat"))
        {
            return typeof(Material);
        }
        else if (uri.EndsWith(".anim"))
        {
            return typeof(AnimationClip);
        }
        return typeof(AssetBundle);
    }

    /// <summary>
    /// 热更新目录
    /// </summary>
    /// <value></value>
    public string updatePath
    {
        get
        {
            return Application.persistentDataPath + "/update/";
        }
    }

    /// <summary>
    /// 拓展包目录
    /// </summary>
    /// <value></value>
    public string extPath
    {
        get
        {
            return Application.persistentDataPath + "/ext/";
        }
    }

    /// <summary>
    /// 内部资源目录
    /// </summary>
    /// <value></value>
    public string internalPath
    {
        get
        {
            return Application.streamingAssetsPath + "/res/";
        }
    }

    /// <summary>
    /// 常规配置AssetBundle (C# 使用的xml配置表)
    /// </summary>
    public AssetBundle m_normalCfgBundle;
    

    private Dictionary<string, Object> m_assets;
    private Dictionary<string, AssetBundle> m_bundles;

    private static AssetBundleMgr s_instance;
    public static AssetBundleMgr instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new AssetBundleMgr();
            return s_instance;
        }
    }
}
