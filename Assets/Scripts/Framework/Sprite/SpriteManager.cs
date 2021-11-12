using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.UI;

public class SpriteManager
{
    public void Init()
    {
        m_cfg.Init();
    }

    public Sprite GetSprite(string spriteName)
    {
        var cfgItem = m_cfg.GetCfg(spriteName);
        if (null == cfgItem)
        {
            GameLogger.LogError("SpriteManager GetSprite Error, null == cfgItem, name: " + spriteName);
            return null;
        }
        var atlas = ResourceManager.instance.Instantiate<SpriteAtlas>(cfgItem.resId);
        if (null == atlas)
        {
            GameLogger.LogError("SpriteManager GetSprite Error, null == atlas, name: " + spriteName);
            return null;
        }
        return atlas.GetSprite(spriteName);
    }

    /// <summary>
    /// 给Image设置精灵图
    /// </summary>
    /// <param name="image">Image对象</param>
    /// <param name="spriteName">精灵图名称</param>
    public void SetSprite(Image image, string spriteName)
    {
        image.sprite = GetSprite(spriteName);
    }

    public void SetSprite(Button btn, string spriteName)
    {
        btn.image.sprite = GetSprite(spriteName);
    }

    public void SetSprite(RawImage rawImage, string spriteName)
    {
        var sprite = GetSprite(spriteName);
        if (null == sprite) return;
        // Sprite转Texture
        var targetTex = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);
        var pixels = sprite.texture.GetPixels(
            (int)sprite.textureRect.x,
            (int)sprite.textureRect.y,
            (int)sprite.textureRect.width,
            (int)sprite.textureRect.height);
        targetTex.SetPixels(pixels);
        targetTex.Apply();

        rawImage.texture = targetTex;
    }

    private SpriteCfg m_cfg = new SpriteCfg();

    private static SpriteManager s_instance;
    public static SpriteManager instance
    {
        get
        {
            if (null == s_instance)
                s_instance = new SpriteManager();
            return s_instance;
        }
    }
}
