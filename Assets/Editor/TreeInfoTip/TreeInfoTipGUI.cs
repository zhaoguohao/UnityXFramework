/********************************************************************
	Created: 2023/01/07
	License Copyright (c) hankangwen
	Author: hankangwen(hankangwen@qq.com)
	Github: https://github.com/hankangwen/Unity-TreeInfoTip
*********************************************************************/

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace TreeInfoTip
{
    [InitializeOnLoad]
    public class TreeInfoTipGUI
    {
        static TreeInfoTipGUI()
        {
            if (!TreeInfoTipManager.Instance.IsOpen) return;
            
            EditorApplication.projectWindowItemOnGUI += HandleOnGUI;
        }

        private static float _column = 5.0f; 
        private static GUIStyle _style;
        private static Color32 _textColor = new Color32(255, 0, 0, 200);
        
        private static void HandleOnGUI(string guid, Rect selectionRect)
        {
            var guid2Title = TreeInfoTipManager.Instance.Guid2Title;
            if (guid2Title.TryGetValue(guid, out string message))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (0 >= path.Length)
                    return;

                bool isDirectory = (File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory;

                string nameRaw;
                if (isDirectory && path.Contains("."))
                {
                    if(!Directory.Exists(path)) return;
                    
                    string[] arrays = path.Split('/');
                    nameRaw = arrays[arrays.Length - 1];
                }
                else
                {
                    if (isDirectory)
                    {
                        if(!Directory.Exists(path)) return;
                    }
                    else
                    {
                        if(!File.Exists(path)) return;
                    }
                    
                    nameRaw = Path.GetFileNameWithoutExtension(path);
                }

                if (_style is null)
                {
                    _style = new GUIStyle(EditorStyles.label);
                }

                _style.normal.textColor = _textColor;
                var extSize = _style.CalcSize(new GUIContent(message));
                var nameSize = _style.CalcSize(new GUIContent(nameRaw));
                selectionRect.x += nameSize.x + (IsSingleColumnView ? 15 : 18) + _column;
                selectionRect.width = nameSize.x + 1 + extSize.x;
            
                var offsetRect = new Rect(selectionRect.position, selectionRect.size);
                EditorGUI.LabelField(offsetRect, message, _style);
            }
        }
        
        private static bool IsSingleColumnView {
            get {
                var projectWindow = GetProjectWindow();
                var columnsCount = (int) projectWindow.GetType().GetField("m_ViewMode", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(projectWindow);
                return columnsCount == 0;
            }
        }
        
        private static EditorWindow GetProjectWindow() {
            if (EditorWindow.focusedWindow != null && EditorWindow.focusedWindow.titleContent.text == "Project") {
                return EditorWindow.focusedWindow;
            }

            return GetExistingWindowByName("Project");
        }
        
        private static EditorWindow GetExistingWindowByName(string name) {
            EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (EditorWindow item in windows) {
                if (item.titleContent.text == name) {
                    return item;
                }
            }

            return default(EditorWindow);
        }
    }
}

