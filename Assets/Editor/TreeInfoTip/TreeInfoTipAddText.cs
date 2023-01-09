/********************************************************************
	Created: 2023/01/07
	License Copyright (c) hankangwen
	Author: hankangwen(hankangwen@qq.com)
	Github: https://github.com/hankangwen/Unity-TreeInfoTip
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;


namespace TreeInfoTip
{
    public class TreeInfoTipAddText : EditorWindow
    {
        [MenuItem("Assets/TreeInfoTip/AddText %#D", true)]
        static bool ValidateShowTreeInfoTipAddText()
        {
            return Selection.objects.Length == 1;
        }

        [MenuItem("Assets/TreeInfoTip/AddText %#D", priority = 1001)]
        static void ShowTreeInfoTipAddText()
        {
            GetWindow<TreeInfoTipAddText>("What Needs To Be Description?").Show();
        }

        private string _selectFilePath;
        private string _showTipStr;
        private string _inputStr;
        private string _guid;
        private bool _isShow = true;
        private void OnFocus()
        {
            Object selectFile = Selection.GetFiltered(typeof(Object), SelectionMode.Assets)[0];
            _selectFilePath = AssetDatabase.GetAssetPath(selectFile);
            _guid = AssetDatabase.AssetPathToGUID(_selectFilePath);
            _showTipStr = $"Input Your {_selectFilePath} Description";
            _inputStr = TreeInfoTipManager.Instance.GetTitleByGuid(_guid);
            _isShow = TreeInfoTipManager.Instance.GetIsShowByGuid(_guid);
        }

        private void OnGUI()
        {
            GUILayout.Label(_showTipStr);
            _inputStr = GUILayout.TextField(_inputStr);
            
            GUILayout.BeginHorizontal();
            _isShow = GUILayout.Toggle(_isShow, "是否显示");
            if (GUILayout.Button("确定"))
            {
                TreeInfoTipManager.Instance.AddToGuid2TipInfo(_selectFilePath, _inputStr, _guid, _isShow);
            }
            if (GUILayout.Button("取消"))
            {
                Close();
            }
            GUILayout.EndHorizontal();
        }
    }
}
