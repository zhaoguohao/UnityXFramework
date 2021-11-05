using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.U2D;
using System.Text;

public class SpriteAtlasTools 
{
    [MenuItem("Tools/命令/生成精灵映射配置")]
    private static void GenSpriteAtlasCfg()
    {
        ResourceManager.instance.Init();

        var fs = Directory.GetFiles(Application.dataPath + "/GameRes/Atlas/");
        Dictionary<string, int> sprite2atlasId = new Dictionary<string, int>();
        StringBuilder sbr = new StringBuilder();
        sbr.AppendLine("<?xml version=\"1.0\" encoding=\"utf-8\"?>");
        sbr.AppendLine("<items>");
        foreach(var f in fs)
        {
            if(!f.EndsWith(".spriteatlas")) continue;
            var atlasFileName = Path.GetFileName(f);
            var assetPath = f.Replace(Application.dataPath , "Assets/");
            var uri = f.Replace(Application.dataPath + "/GameRes/", "");
            var atals = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(assetPath);
            var resId = ResourceManager.instance.Uri2Id(uri);
            Sprite[] sprites = new Sprite[atals.spriteCount];
            atals.GetSprites(sprites);
            foreach(var sprite in sprites)
            {
                var key = sprite.texture.name;
                if(sprite2atlasId.ContainsKey(key))
                    GameLogger.LogError("精灵名重复, key: " + key);
                else
                {
                    sprite2atlasId.Add(sprite.texture.name, resId);
                    sbr.AppendLine(string.Format("  <item name=\"{0}\" atlas=\"{1}\" resId=\"{2}\"/>", key, atlasFileName, resId));
                }
            }
        }
        sbr.AppendLine("</items>");
        // 保存配置
        // GameLogger.Log(sbr.ToString());
        SaveXmlCfg(sbr.ToString(), Application.dataPath + "/GameRes/Config/sprite2atlas.bytes");
    }

    private static void SaveXmlCfg(string txt, string path)
    {
        using(FileStream s = new FileStream(path, FileMode.OpenOrCreate))        
        {
            StreamWriter writer = new StreamWriter(s);
            writer.Write(txt);
            writer.Close();
        }
        GameLogger.LogGreen("gen sprite2atlas cfg done, path: " + path);
    }
}
