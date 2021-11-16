using UnityEditor;
using UnityEngine;
using System.Xml;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public enum OperType
{
    Add,
    Update,
    Delete
}

public enum ResEditorErrorCode
{
    Ok = -1,
    DescriptionNull = 1,
    ObjNull,
    ResExist,
    PathError,
    ArgsError,
    DuplicateID,
    UnknowError
}

public class ResConfigEditor : EditorWindow
{
    [MenuItem("Tools/Aux/资源添加编辑器 &g")]
    static void Init()
    {
        ResConfigEditor window = (ResConfigEditor)EditorWindow.GetWindow(typeof(ResConfigEditor));
        //window.minSize = new Vector2(500f, 600f);
        window.Show();
        s_manager = new ResConfigManager();
    }

    void OnDestroy()
    {
    }

    void OnGUI()
    {
        GUILayout.Label("资源配置", EditorStyles.boldLabel);

        EditorGUILayout.BeginVertical();
        if (m_curObj != m_lastObj)
        {
            m_lastObj = m_curObj;
            m_operType = s_manager.exist(m_curObj.name) ? OperType.Update : OperType.Add;
            string path = AssetDatabase.GetAssetPath(m_curObj);
            if (!ResConfigManager.isPathRight(path)) m_errorCode = ResEditorErrorCode.PathError;

            if (m_operType == OperType.Update)
            {
                string filePath;
                XmlElement element;
                s_manager.getResourceElement(m_curObj, out filePath, out element);
                m_resID = int.Parse(element.Attributes["id"].Value);
                m_description = element.Attributes["desc"].Value;
            }
            else if (m_operType == OperType.Add)
            {
                m_resID = -1;
                m_description = "";
            }
        }

        m_curObj = EditorGUILayout.ObjectField("资源对象", m_curObj, typeof(Object), false, m_option);

        m_operType = (OperType)EditorGUILayout.EnumPopup("操作类型", m_operType);

        if (m_operType == OperType.Add || m_operType == OperType.Update)
        {

            m_description = EditorGUILayout.TextField("资源描述", m_description, m_option);
            EditorGUILayout.SelectableLabel("自动生成的资源ID：" + m_resID.ToString(), m_option);

            if (m_operType == OperType.Add && GUILayout.Button("Add"))
            {
                m_errorCode = s_manager.append(m_curObj, m_description, m_arg, out m_resID);
            }

            if (m_operType == OperType.Update && GUILayout.Button("Update"))
            {
                m_errorCode = s_manager.update(m_curObj, m_description, m_arg);
            }
        }
        else
        {
            m_resourceName = EditorGUILayout.TextField("资源名称", m_resourceName, m_option);
            if (GUILayout.Button("Delete"))
            {
                s_manager.delete(m_curObj, m_resourceName);
            }
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.Space();
        EditorGUILayout.SelectableLabel(ResConfigManager.getTipsByID(m_errorCode), m_option);
        EditorGUILayout.EndVertical();
    }

    private static ResConfigManager s_manager;
    private Object m_curObj = null, m_lastObj = null;

    private OperType m_operType = OperType.Add;
    private string m_resourceName = string.Empty;

    private string m_description = string.Empty;
    private int m_resID = -1;
    private ResEditorErrorCode m_errorCode = ResEditorErrorCode.Ok;
    private float m_arg = 1f;

    GUILayoutOption[] m_option = new GUILayoutOption[] { GUILayout.Width(300), GUILayout.Height(16) };
}

public class ResConfigManager
{
    public ResConfigManager()
    {
        initResourceConf();
    }

    public ResEditorErrorCode append(Object obj, string description, float arg, out int resourceID)
    {
        resourceID = -1;

        if (string.IsNullOrEmpty(description))
        {
            return ResEditorErrorCode.DescriptionNull;
        }

        if (obj == null)
        {
            return ResEditorErrorCode.ObjNull;
        }

        if (exist(obj.name)) return ResEditorErrorCode.ResExist;
        string path = AssetDatabase.GetAssetPath(obj);
        if (!isPathRight(path)) return ResEditorErrorCode.PathError;
        var ret = ResEditorErrorCode.Ok;
        resourceID = appendResourceConf("resources", s_itemMap1, path, description);
        if (path.EndsWith(".ogg") || path.EndsWith(".wav"))
        {
            var fname = Path.GetFileName(path);
            ret = appendAudioConf(fname, resourceID, (float)arg);
        }
        return ret;
    }

    public ResEditorErrorCode getResourceConfigItem(string name, out ResConfigItem cfg)
    {
        cfg = null;

        foreach (ResConfigItem item in s_itemMap1.Values)
        {
            if (item != null && name.Equals(item.name))
            {
                cfg = item;
                return ResEditorErrorCode.Ok;
            }
        }

        return ResEditorErrorCode.ObjNull;
    }

    public ResEditorErrorCode getResourceConfigItem(int id, out ResConfigItem cfg)
    {
        cfg = null;

        if (s_itemMap1.TryGetValue(id, out cfg))
        {
            return ResEditorErrorCode.Ok;
        }

        return ResEditorErrorCode.ObjNull;
    }

    private XmlDocument getResourceElement(string name, out string filePath, out XmlElement item)
    {
        filePath = ResourcePathBuilder.BuildConfigPath("resources");
        XmlDocument doc = getConfElementByName(name, filePath, out item);
        return doc;
    }

    public XmlDocument getResourceElement(Object obj, out string filePath, out XmlElement item)
    {
        return getResourceElement(obj.name, out filePath, out item);
    }


    public ResEditorErrorCode update(Object obj, string description, float arg)
    {
        XmlDocument doc = null;
        XmlElement item = null;
        string filePath = string.Empty;
        doc = getResourceElement(obj, out filePath, out item);
        if (doc == null || item == null) return ResEditorErrorCode.ObjNull;

        string sourcepath = AssetDatabase.GetAssetPath(obj);
        string path;

        path = sourcepath.Replace("Assets/GameRes/", "");
        item.SetAttribute("editorPath", path);

        item.SetAttribute("version", System.DateTime.Now.ToShortDateString());
        item.SetAttribute("desc", description);
        StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(false));
        doc.Save(sw);
        sw.Close();
        if (path.EndsWith(".ogg") || path.EndsWith(".wav"))
        {
            string fname = Path.GetFileName(path);
            int resid = int.Parse(item.GetAttribute("id"));
            updateAudioConf(fname, resid, arg);
        }


        int resourceID = int.Parse(item.Attributes["id"].Value);
        if (s_itemMap1.ContainsKey(resourceID))
        {
            ResConfigItem cfg = s_itemMap1[resourceID];
            cfg.path = path;
            cfg.desc = description;
        }
        return ResEditorErrorCode.Ok;
    }

    private XmlDocument getConfElementByName(string objName, string filePath, out XmlElement element)
    {
        element = null;
        XmlDocument doc = XMLUtil.getXMLFromFile(filePath);
        XmlNodeList nodeList = doc.GetElementsByTagName("item");
        if (nodeList != null)
        {
            for (int i = 0; i < nodeList.Count; ++i)
            {
                string editorPath = nodeList[i].Attributes["editorPath"].Value;
                int index1 = editorPath.LastIndexOf('/');
                if (index1 == -1) continue;
                string tempPath = editorPath.Remove(0, index1 + 1);
                int index2 = tempPath.LastIndexOf('.');
                if (index2 == -1) continue;
                string tempName = tempPath.Remove(index2, tempPath.Length - index2);
                if (objName.Equals(tempName))
                {
                    element = (XmlElement)nodeList[i];
                    break;
                }
            }
        }

        return doc;
    }

    public ResEditorErrorCode delete(Object obj, string name)
    {
        int resourceID = -1;
        ResEditorErrorCode errorCode = ResEditorErrorCode.UnknowError;
        errorCode = deleteResouceConf(obj != null ? obj.name : name, out resourceID);
        if (errorCode == ResEditorErrorCode.Ok)
        {

            if (obj.name.EndsWith(".ogg") || obj.name.EndsWith(".wav"))
            {
                errorCode = deleteItemInConf("audioConfig", "items", "audio", "id", resourceID);
            }
        }

        return errorCode;
    }

    private ResEditorErrorCode deleteResouceConf(string name, out int resourceID)
    {
        resourceID = -1;
        if (string.IsNullOrEmpty(name)) return ResEditorErrorCode.ArgsError;
        string filePath = string.Empty;
        XmlElement element = null;
        XmlDocument doc = getResourceElement(name, out filePath, out element);
        resourceID = int.Parse(element.Attributes["id"].Value);
        XmlNode items = doc.SelectSingleNode("items");
        items.RemoveChild(element);
        StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(false));
        doc.Save(sw);
        sw.Close();
        if (s_itemMap1.ContainsKey(resourceID))
            s_itemMap1.Remove(resourceID);
        AssetDatabase.Refresh();
        return ResEditorErrorCode.Ok;
    }

    private ResEditorErrorCode deleteItemInConf(string fileName, string rootItem, string item, string resIDArrtibute, int resourceID)
    {
        if (resourceID == -1)
        {
            GameLogger.LogError("resourceID error.");
            return ResEditorErrorCode.ArgsError;
        }

        string filePath = ResourcePathBuilder.BuildConfigPath(fileName);
        XmlDocument doc = XMLUtil.getXMLFromFile(filePath);
        XmlNode rootNode = doc.SelectSingleNode(rootItem);
        XmlNodeList items = doc.GetElementsByTagName(item);
        XmlNode childNode = null;
        foreach (XmlNode node in items)
        {
            int tempResourceID = int.Parse(node.Attributes[resIDArrtibute].Value);
            if (tempResourceID == resourceID)
            {
                childNode = node;
                break;
            }
        }

        childNode = rootNode.RemoveChild(childNode);
        if (childNode == null)
            GameLogger.LogError("remove error ...");
        StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(false));
        doc.Save(sw);
        sw.Close();
        AssetDatabase.Refresh();
        return ResEditorErrorCode.Ok;
    }

    private int appendResourceConf(string resourceConf, Dictionary<int, ResConfigItem> map1,
        string sourcepath, string description)
    {
        int id = map1.Count > 0 ? map1.Keys.Max() + 1 : 1;
        string filePath = ResourcePathBuilder.BuildConfigPath(resourceConf);
        XmlDocument doc = XMLUtil.getXMLFromFile(filePath);
        XmlNode rootNode = doc.SelectSingleNode("items");
        XmlElement item = doc.CreateElement("item");
        rootNode.AppendChild(item);
        item.SetAttribute("id", id.ToString());

        string path = sourcepath.Replace("Assets/GameRes/", "");
        item.SetAttribute("editorPath", path);
        item.SetAttribute("version", System.DateTime.Now.ToShortDateString());
        item.SetAttribute("desc", description);
        StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(false));
        doc.Save(sw);
        sw.Close();
        AssetDatabase.Refresh();
        map1.Add(id, new ResConfigItem(id, sourcepath, description));
        return id;
    }

    private bool isDuplicate(XmlDocument doc, string item, string attribute, int id)
    {
        XmlNodeList list = doc.GetElementsByTagName(item);
        foreach (XmlNode node in list)
        {
            int tempid = int.Parse(node.Attributes[attribute].Value);
            if (tempid == id)
                return true;
        }

        return false;
    }

    private ResEditorErrorCode appendAudioConf(string fname, int resID, float volume)
    {
        string filePath = ResourcePathBuilder.BuildConfigPath("audioConfig");
        XmlDocument doc = XMLUtil.getXMLFromFile(filePath);
        if (isDuplicate(doc, "audio", "id", resID))
            return ResEditorErrorCode.DuplicateID;
        XmlNode rootNode = doc.SelectSingleNode("items");
        XmlElement item = doc.CreateElement("audio");
        rootNode.AppendChild(item);
        item.SetAttribute("id", resID.ToString());
        item.SetAttribute("name", fname);
        item.SetAttribute("volume", volume.ToString());
        AssetDatabase.Refresh();
        StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(false));
        doc.Save(sw);
        sw.Close();
        return ResEditorErrorCode.Ok;
    }

    private ResEditorErrorCode updateAudioConf(string fname, int resID, float volume)
    {
        string filePath = ResourcePathBuilder.BuildConfigPath("audioConfig");
        XmlDocument doc = XMLUtil.getXMLFromFile(filePath);
        if (isDuplicate(doc, "audio", "id", resID))
            return ResEditorErrorCode.DuplicateID;
        XmlNode rootNode = doc.SelectSingleNode("items");
        XmlNodeList audioNodes = rootNode.SelectNodes("audio");
        bool isUpdate = false;
        for (int i = 0, count = audioNodes.Count; i < count; ++i)
        {
            XmlNode audionode = audioNodes[i];
            if (audionode.Attributes["id"].Value == resID.ToString() && audionode.Attributes["name"].Value == fname)
            {
                audionode.Attributes["id"].Value = resID.ToString();
                audionode.Attributes["name"].Value = fname;
                audionode.Attributes["volume"].Value = volume.ToString();
                isUpdate = true;
                break;
            }
        }

        if (!isUpdate)
        {
            Debug.LogWarning(string.Format("资源编辑器更新音效时，没有在audioCoinfig.bytes文件中找到音效{0}，所以在这里添加此音效！", fname));
            XmlElement item = doc.CreateElement("audio");
            rootNode.AppendChild(item);
            item.SetAttribute("id", resID.ToString());
            item.SetAttribute("name", fname);
            item.SetAttribute("volume", volume.ToString());
        }

        AssetDatabase.Refresh();
        StreamWriter sw = new StreamWriter(filePath, false, new UTF8Encoding(false));
        doc.Save(sw);
        sw.Close();
        return ResEditorErrorCode.Ok;
    }

    public bool exist(string name)
    {
        foreach (ResConfigItem item in s_itemMap1.Values)
        {
            if (item != null && name.Equals(item.name))
                return true;
        }

        return false;
    }

    public bool ExistResourceConfigItem(string path, out ResConfigItem configItem)
    {
        foreach (ResConfigItem item in s_itemMap1.Values)
        {
            if (item != null && path.Equals(item.path))
            {
                configItem = item;
                return true;
            }
        }
        configItem = null;
        return false;
    }

    private static string joinIntArray(int[] values)
    {
        StringBuilder sb = new StringBuilder();
        if (values != null)
        {
            foreach (int value in values)
            {
                if (sb.Length > 0) sb.Append(",");
                sb.Append(value);
            }
        }
        return sb.ToString();
    }

    private static void initResourceConf()
    {
        s_itemMap1 = new Dictionary<int, ResConfigItem>();
        initResourceConf("resources", s_itemMap1);
    }

    private static void initResourceConf(string fileName, Dictionary<int, ResConfigItem> map)
    {
        string filePath = ResourcePathBuilder.BuildConfigPath(fileName);
        XmlDocument doc = XMLUtil.getXMLFromFile(filePath);
        XmlNodeList nodeList = doc.GetElementsByTagName("item");
        if (nodeList == null || nodeList.Count == 0) return;
        for (int i = 0; i < nodeList.Count; ++i)
        {
            XmlNode currentNode = nodeList.Item(i);
            int id = XmlConvert.ToInt32(currentNode.Attributes["id"].Value);
            string editorPath = currentNode.Attributes["editorPath"].Value;
            string description = currentNode.Attributes["desc"] != null ? currentNode.Attributes["desc"].Value : "请填写资源描述,便于维护.";

            map.Add(id, new ResConfigItem(id, editorPath, description));
        }
    }

    public static bool isPathRight(string path)
    {
        return path.Contains("GameRes");
    }

    public static string getTipsByID(ResEditorErrorCode code)
    {
        switch (code)
        {
            case ResEditorErrorCode.Ok:
                return "这是游戏资源配置编辑器。";
            case ResEditorErrorCode.DescriptionNull:
                return "资源描述不能为空！";
            case ResEditorErrorCode.ObjNull:
                return "请将需要添加的资源拖放到资源对象项。";
            case ResEditorErrorCode.ResExist:
                return "同名资源已存在,需要进行更新操作？";
            case ResEditorErrorCode.PathError:
                return "资源放置的路径不正确,不清楚请问程序猿。";
            case ResEditorErrorCode.ArgsError:
                return "参数错误。";
            case ResEditorErrorCode.DuplicateID:
                return "ID重复！！！";
            default:
                return "sorry, 未知错误";
        }
    }


    private static Dictionary<int, ResConfigItem> s_itemMap1;
}

public class ResConfigItem
{
    public ResConfigItem(int resourceID, string resourcePath,
        string resourceDescription = null)
    {
        this.id = resourceID;
        this.path = resourcePath;
        this.desc = resourceDescription;
        parseName();
    }

    private void parseName()
    {
        int index0 = this.path.LastIndexOf(".");
        int index1 = this.path.LastIndexOf("/");
        if (index0 == -1 && index1 != -1)
        {
            this.name = this.path.Substring(index1 + 1, this.path.Length - index1 - 1);
        }
        else
        {
            this.name = this.path.Substring(index1 + 1, index0 - index1 - 1);
        }
    }

    public int id;
    public string path;
    public string desc;
    public string name;
}