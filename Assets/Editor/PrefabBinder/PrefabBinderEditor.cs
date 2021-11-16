
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

public class PrefabBinderEditor : EditorWindow
{
    private GameObject m_prefabBinderObj;
    private PrefabBinder m_slot;
    private List<PrefabBinder.Item> m_itemList;
    private List<PrefabBinder.Item> m_searchMatchItemList = new List<PrefabBinder.Item>();
    private Vector2 m_scrollViewPos;
    private List<Component> m_comList = new List<Component>();
    private string m_itemName;



    private string m_itemNameSearch;
    private string m_selectedItemName;
    private string m_lockBtnName;
    private Object m_itemObj;
    private bool m_lock;
    private string m_componentStr="";
    enum ItemOption
    {
        AddItem,
        RemoveItem,
        ClearItems,
        SearchItems
    }

    private GUIStyle m_labelSytleYellow;
    private GUIStyle m_labelStyleNormal;


    [MenuItem("Tools/Aux/PrefabBinder工具")]
    public static void ShowWindow()
    {
        var window = GetWindow<PrefabBinderEditor>();
        window.titleContent = new GUIContent("PrefabBinder", AssetPreview.GetMiniTypeThumbnail(typeof(UnityEngine.EventSystems.EventSystem)), "decent");
        window.Init();
    }

    [MenuItem("GameObject/PrefabBinder Window", priority = 0)]
    public static void PrefabBinderWindow()
    {
        if (Selection.activeGameObject.GetComponent<PrefabBinder>())
            ShowWindow();
        else
            Debug.LogError("no PrefabBinder on this GameObject");
    }

    void Awake()
    {
        m_labelStyleNormal = new GUIStyle(EditorStyles.miniButton);
        m_labelStyleNormal.fontSize = 12;
        m_labelStyleNormal.normal.textColor = Color.white;

        m_labelSytleYellow = new GUIStyle(EditorStyles.miniButton);
        m_labelSytleYellow.fontSize = 12;
        m_labelSytleYellow.normal.textColor = Color.yellow;
        
    }

    void OnEnable()
    {
        EditorApplication.update += Repaint;
    }

    void OnDisable()
    {
        EditorApplication.update -= Repaint;
    }

    void Init()
    {
        m_itemList = new List<PrefabBinder.Item>();
        m_comList = new List<Component>();
        m_lockBtnName = "锁定item组件列表";
        m_componentStr = string.Empty;
        m_lock = false;
        if (Selection.activeGameObject.GetComponent<PrefabBinder>())
        {
            m_prefabBinderObj = Selection.activeGameObject;
            OnRefreshBtnClicked();
        }
    }

    void OnGUI()
    {
        float offset = 0;
        float width = 3 * Screen.width / 10f;
        BeginBox(new Rect(offset, 0, width, Screen.height));
        DrawSearchBtn();
        DrawSearchItemList();
        EndBox();
        offset += width;

        width =  2 * Screen.width / 10f;
        BeginBox(new Rect(offset, 0, width, Screen.height));
        DrawLockBtn();
        GUILayout.Space(2);
        DrawComponentList();
        EndBox();
        offset += width;

        width = 3 * Screen.width / 10f;
        BeginBox(new Rect(offset, 0, width, Screen.height));
        DrawPrefabBinderField();
        GUILayout.Space(2);
        DrawItemField(width);
        EndBox();
    }

    private void DrawSearchBtn()
    {
        GUILayout.BeginHorizontal();
        string before = m_itemNameSearch;
        string after = EditorGUILayout.TextField("", before, "SearchTextField");
        if (before != after) m_itemNameSearch = after;

        if (GUILayout.Button("", "SearchCancelButton"))
        {
            m_itemNameSearch = "";
            GUIUtility.keyboardControl = 0;
        }
        ComponentOperation(m_slot, ItemOption.SearchItems, after);
        GUILayout.EndHorizontal();
    }

    private void DrawPrefabBinderField()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("binder");
        var oldObj = m_prefabBinderObj;
        m_prefabBinderObj = EditorGUILayout.ObjectField(m_prefabBinderObj, typeof(GameObject), true) as GameObject;

  
        EditorGUILayout.EndHorizontal();
        if (!m_prefabBinderObj)
        {
            EditorGUILayout.HelpBox("Select a PrefabBinder Object", MessageType.Warning);
        }
        else if (oldObj != m_prefabBinderObj)
        {
            m_slot = m_prefabBinderObj.GetComponent<PrefabBinder>();
        }
    }

    private void BeginBox(Rect rect)
    {
        rect.height -= 20;
        GUILayout.BeginArea(rect);
        GUILayout.Box("", GUILayout.Width(rect.width), GUILayout.Height(rect.height));
        GUILayout.EndArea();
        GUILayout.BeginArea(rect);
    }

    private void EndBox()
    {
        GUILayout.EndArea();
    }

    private void DrawSearchItemList()
    {
        if (null == m_prefabBinderObj || null == m_slot)
            m_searchMatchItemList.Clear();
        m_scrollViewPos = EditorGUILayout.BeginScrollView(m_scrollViewPos);
        foreach (var item in m_searchMatchItemList)
        {
            GUILayout.BeginHorizontal();
            item.name = EditorGUILayout.TextField(item.name);
            item.obj = EditorGUILayout.ObjectField(item.obj, typeof(GameObject), true);
            if (GUILayout.Button("-", GUILayout.Width(20)))
            {
                m_itemList.Remove(item);
                m_slot.items = m_itemList.ToArray();
                GUILayout.EndHorizontal();
                break;
            }
            GUILayout.EndHorizontal();
        }
        EditorGUILayout.EndScrollView();
    }

    private void DrawItemField(float width)
    {
        EditorGUILayout.BeginVertical();

        GUILayout.Label(string.IsNullOrEmpty(m_componentStr) ? "null" : m_componentStr);
        m_itemName = EditorGUILayout.TextField(m_itemName);
        if (GUILayout.Button("Add Item", GUILayout.Width(width), GUILayout.Height(80)))
        {
            ComponentOperation(m_slot, ItemOption.AddItem);
        }
        if (GUILayout.Button("Delete Item",  GUILayout.Width(width)))
        {
            if (m_prefabBinderObj != null)
            {
                if (string.IsNullOrEmpty(m_itemName))
                    Debug.LogWarning("请输入要删除的项目名称");
                else
                    ComponentOperation(m_slot, ItemOption.RemoveItem);
            }
        }
        if (GUILayout.Button("Refresh",  GUILayout.Width(width)))
        {
            OnRefreshBtnClicked();
        }
        ItemTip();
    }

    private void OnRefreshBtnClicked()
    {
        if (null != m_prefabBinderObj)
            m_slot = m_prefabBinderObj.GetComponent<PrefabBinder>();
        if (null == m_slot)
        {
            m_itemList.Clear();
            m_comList.Clear();
        }
    }

    private void DrawLockBtn()
    {
        if (GUILayout.Button(new GUIContent(m_lockBtnName, m_lockBtnName), EditorStyles.toolbarButton))
        {
            m_lock = !m_lock;
            if (m_lock == false)
                m_lockBtnName = "锁定item组件列表";
            else
                m_lockBtnName = "解锁item组件列表";
        }
    }

    private void DrawComponentList()
    {
        var go = Selection.activeObject as GameObject; //获取选中对象
        if (go && m_lock == false)
        {
            Component[] components = go.GetComponents<Component>();
            m_comList.Clear();
            m_comList.AddRange(components);
            m_selectedItemName = go.name;
        }
        
        if (go == null)
        {
            m_comList.Clear();
            m_selectedItemName = "无选中对象";
        }

        if (go && GUILayout.Button("GameObject", "GameObject" == m_componentStr ? m_labelSytleYellow : m_labelStyleNormal))
        {
            m_itemObj = go;
            m_componentStr = "GameObject";
        }

        foreach (var com in m_comList)
        {

            GUILayout.Space(2);
            var comType = com.GetType().ToString();
            comType = comType.Replace("UnityEngine.UI.", "");
            comType = comType.Replace("UnityEngine.", "");
            if (GUILayout.Button(comType, comType == m_componentStr ? m_labelSytleYellow : m_labelStyleNormal))
            {
                m_itemObj = com;
                m_componentStr = comType;
            }
        }

        EditorGUILayout.EndVertical();
    }

    #region private method
    private void ComponentOperation(PrefabBinder slot, ItemOption option, string name = " ")
    {
        if (null == slot) return;
        PrefabBinder.Item item = new PrefabBinder.Item();
        switch (option)
        {
            case ItemOption.AddItem:
                AddItem(item, slot);
                break;

            case ItemOption.RemoveItem:
                RemoveItem(item, slot);
                break;

            case ItemOption.ClearItems:
                ClearItem(item, slot);
                break;

            case ItemOption.SearchItems:
                SearchItem(item, slot, name);
                break;
        }
        slot.items = m_itemList.ToArray();
    }

    private void AddItem(PrefabBinder.Item item, PrefabBinder Ps)
    {
        item.name = m_itemName;
        item.obj = m_itemObj;
        m_itemList = Ps.items.ToList();
        List<string> nameList = new List<string>();
        foreach (var iL in m_itemList)
        {
            nameList.Add(iL.name);
        }
        if (!string.IsNullOrEmpty(m_itemName) && m_itemObj != null)
        {
            if (nameList.Contains(m_itemName))
            {
                Debug.LogError("重复元素");
                m_itemList.Add(item);
            }
            else
                m_itemList.Add(item);
        }
    }

    private void RemoveItem(PrefabBinder.Item item, PrefabBinder Ps)
    {
        item.name = m_itemName;

        m_itemList = Ps.items.ToList();
        for (int i = 0; i < m_itemList.Count; i++)
        {
            if (m_itemList[i].name.ToLower() == item.name.ToLower())
            {
                m_itemList.Remove(m_itemList[i]);
                break;
            }
        }
    }

    private void ClearItem(PrefabBinder.Item item, PrefabBinder Ps)
    {
        item.name = m_itemName;
        item.obj = m_itemObj;
        m_itemList = Ps.items.ToList();

        for (int i = 0; i < m_itemList.Count; i++)
        {
            if (m_itemList[i].obj == null || string.IsNullOrEmpty(m_itemList[i].name))
            {
                m_itemList.Remove(m_itemList[i]);
            }
        }
    }

    private void SearchItem(PrefabBinder.Item item, PrefabBinder slot, string name)
    {
        m_itemList = slot.items.ToList();
        m_searchMatchItemList.Clear();

        foreach (var o in m_itemList)
        {
            if (string.IsNullOrEmpty(name))
            {
                m_searchMatchItemList.Add(o);
            }
            else
            {
                if (o.name.ToLower().Contains(name.ToLower()))
                {
                    m_searchMatchItemList.Add(o);
                }
                else if (null != o.obj)
                {
                    var objName = o.obj.name;
                    if (objName.ToLower().Contains(name.ToLower()))
                    {
                        m_searchMatchItemList.Add(o);
                    }
                }
            }
        }
    }

    private void ItemTip()
    {
        if (string.IsNullOrEmpty(m_itemName) || m_itemObj == null)
        {
            string msg = string.Empty;
            if (m_itemObj == null)
            {
                msg = "请选择项目组件";
            }
            else if (string.IsNullOrEmpty(m_itemName))
            {
                msg = "请输入要添加的项的名字";
            }

            EditorGUILayout.HelpBox(msg, MessageType.Warning);
            EditorGUILayout.Space();
        }
    }

    #endregion
}
