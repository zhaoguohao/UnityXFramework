/// <summary>
/// EditorSytleDef
/// </summary>

using UnityEngine;
using UnityEditor;

public class EditorStyleDef
{
    public static GUIStyle labelStyleNormal
    {
        get
        {
            if (m_labelStyleNormal == null)
            {
                m_labelStyleNormal = new GUIStyle(EditorStyles.label);
                m_labelStyleNormal.fontSize = 12;
                m_labelStyleNormal.normal.textColor = Color.white;
            }
            return m_labelStyleNormal;
        }
    }

    public static GUIStyle labelStyleBlue
    {
        get
        {
            if (m_labelStyleBlue == null)
            {
                m_labelStyleBlue = new GUIStyle(labelStyleNormal);
                Texture2D tex = CreateTexture2D(new Color(35 / 255f, 55 / 255f, 75 / 255f));
                m_labelStyleBlue.normal.background = tex;
                m_labelStyleBlue.normal.textColor = Color.white;
            }
            return m_labelStyleBlue;
        }
    }
    public static GUIStyle labelSytleGreen
    {
        get
        {
            if (m_labelStyleYellow == null)
            {
                m_labelStyleYellow = new GUIStyle(labelStyleNormal);
                Texture2D tex = CreateTexture2D(new Color(185f / 255f, 1, 144 / 255f));
                m_labelStyleYellow.normal.background = tex;
                m_labelStyleYellow.normal.textColor = Color.black;
                m_labelStyleYellow.fontStyle = FontStyle.Bold;
            }
            return m_labelStyleYellow;
        }
    }


    public static GUIStyle labelSytleYellow
    {
        get
        {
            if (m_labelStyleGreen == null)
            {
                m_labelStyleGreen = new GUIStyle(labelStyleNormal);
                Texture2D tex = CreateTexture2D(Color.yellow);
                m_labelStyleGreen.normal.background = tex;
                m_labelStyleGreen.normal.textColor = Color.black;
            }
            return m_labelStyleGreen;
        }
    }

    public static GUIStyle labelSytleGray
    {
        get
        {
            if (m_labelSytleGray == null)
            {
                m_labelSytleGray = new GUIStyle(labelStyleNormal);
                Texture2D tex = CreateTexture2D(Color.gray);
                m_labelSytleGray.normal.background = tex;
                m_labelSytleGray.normal.textColor = Color.black;
                m_labelSytleGray.wordWrap = true;
            }
            return m_labelSytleGray;
        }
    }

    private static Texture2D CreateTexture2D(Color color)
    {
        Texture2D tex = new Texture2D(1, 1);
        tex.SetPixel(0, 0, color);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.Apply();
        return tex;
    }

    public static GUIStyle boxStyleGrayBlue
    {
        get
        {
            if (m_boxStyleGrayBlue == null)
            {
                m_boxStyleGrayBlue = new GUIStyle(EditorStyles.helpBox);
                Texture2D tex = CreateTexture2D(new Color(35 / 255f, 55 / 255f, 75 / 255f));
                m_boxStyleGrayBlue.normal.background = tex;
            }

            return m_boxStyleGrayBlue;
        }
    }

    public static GUIStyle boxStyleGrayYellow
    {
        get
        {
            if (m_boxStyleGrayYellow == null)
            {
                m_boxStyleGrayYellow = new GUIStyle(EditorStyles.helpBox);
                Texture2D tex = CreateTexture2D(new Color(17 / 255f, 30 / 255f, 41 / 255f));
                m_boxStyleGrayYellow.normal.background = tex;
            }
            return m_boxStyleGrayYellow;
        }
    }

    public static GUIStyle textAreaNormal
    {
        get
        {
            if (null == m_textAreaNormal)
            {
                m_textAreaNormal = new GUIStyle(EditorStyles.textArea);
                m_textAreaNormal.wordWrap = true;
            }

            return m_textAreaNormal;
        }
    }

    public static GUIStyle numberField
    {
        get
        {
            if(null == m_numberField)
            {
                m_numberField = new GUIStyle(EditorStyles.numberField);
                m_numberField.alignment = TextAnchor.MiddleLeft;
                m_numberField.overflow = new RectOffset(130, 0, 0, 0);
                m_numberField.padding = new RectOffset(-130, 0, 0, 0);
            }
            return m_numberField;
        }
    }

    private static GUIStyle m_labelStyleNormal;
    private static GUIStyle m_labelStyleBlue;
    private static GUIStyle m_labelStyleGreen;
    private static GUIStyle m_labelStyleYellow;
    private static GUIStyle m_labelSytleGray;
    private static GUIStyle m_boxStyleGrayBlue;
    private static GUIStyle m_boxStyleGrayYellow;

    private static GUIStyle m_textAreaNormal;

    private static GUIStyle m_numberField;
}
