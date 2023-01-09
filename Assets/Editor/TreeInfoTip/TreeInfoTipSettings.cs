/********************************************************************
	Created: 2023/01/07
	License Copyright (c) hankangwen
	Author: hankangwen(hankangwen@qq.com)
	Github: https://github.com/hankangwen/Unity-TreeInfoTip
*********************************************************************/

using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TreeInfoTip
{
    public class TreeInfoTipSettings : EditorWindow
    {
        [MenuItem("Assets/TreeInfoTip/Settings", priority = 1000)]
        static void ShowTreeInfoTipEditor()
        {
            GetWindow<TreeInfoTipSettings>("TreeInfoTip-Settings").Show();
        }

        
        private Object xml_DirectoryV2;
        
        private void OnEnable()
        {
            string path = TreeInfoTipManager.Instance.DirectoryV2Path;
            xml_DirectoryV2 = AssetDatabase.LoadAssetAtPath<Object>(path.Substring(path.IndexOf("Assets", StringComparison.Ordinal)));
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal();
            xml_DirectoryV2 = EditorGUILayout.ObjectField("目录或图片", xml_DirectoryV2, typeof(Object), false);
            if (GUILayout.Button("修改xml路径"))
            {
                TreeInfoTipManager.Instance.DirectoryV2Path = AssetDatabase.GetAssetPath(xml_DirectoryV2);
            }
            EditorGUILayout.EndHorizontal();
            
            if (GUILayout.Button("打开"))
            {
                
            }
            if (GUILayout.Button("关闭"))
            {
                
            }
        }
    }
}

