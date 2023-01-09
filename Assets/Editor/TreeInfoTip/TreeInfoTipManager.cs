/********************************************************************
	Created: 2023/01/07
	License Copyright (c) hankangwen
	Author: hankangwen(hankangwen@qq.com)
	Github: https://github.com/hankangwen/Unity-TreeInfoTip
*********************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace TreeInfoTip
{
    public class TreeInfoTipManager
    {
        private static TreeInfoTipManager _instance;

        public static TreeInfoTipManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new TreeInfoTipManager();
                return _instance;
            }
        }

        #region DirectoryV2
        private TreeInfoTipManager()
        {
            _directoryV2Path = EditorPrefs.GetString(_directoryV2SaveKey, _directoryV2Path);
        }

        private string _directoryV2SaveKey = "TreeInfoTip_DirectoryV2";
        private string _directoryV2Path = Application.dataPath + "/Editor/TreeInfoTip/DirectoryV2.xml";
        public string DirectoryV2Path
        {
            get => _directoryV2Path;
            set
            {
                if (!File.Exists(value))
                {
                    Debug.LogError($"Can not find file {value}, please check again.");
                    return;
                }
                _directoryV2Path = value;
                EditorPrefs.SetString(_directoryV2SaveKey, _directoryV2Path);
                _guid2TipInfo = null;
            }
        }
        #endregion
        
        
        public bool IsOpen = true;  //是否开启TreeInfoTip
        
        private Dictionary<string, TipInfo> _guid2TipInfo;
        public Dictionary<string, TipInfo> Guid2TipInfo
        {
            get
            {
                if (_guid2TipInfo == null) 
                    CreateGuid2TipInfo();
                return _guid2TipInfo;
            }
        }

        private readonly string PATH = "path";
        private readonly string TITLE = "title";
        private readonly string GUID = "guid";
        private readonly string IS_SHOW = "isShow";

        public bool AddToGuid2TipInfo(string path, string title, string guid, bool isShow)
        {
            TipInfo info = new TipInfo(path, title, guid, isShow);
            if (_guid2TipInfo.ContainsKey(guid))
            {
                _guid2TipInfo[guid] = info;
                UpdateDirectoryV2(info);
            }
            else
            {
                _guid2TipInfo.Add(guid, info);
                AddDirectoryV2(info);
            }

            return true;
        }

        public string GetTitleByGuid(string guid)
        {
            if (Guid2TipInfo.TryGetValue(guid, out TipInfo info))
            {
                return info.title;
            }
            return String.Empty;
        }

        public bool GetIsShowByGuid(string guid)
        {
            if (Guid2TipInfo.TryGetValue(guid, out TipInfo info))
            {
                return info.isShow;
            }
            return true;
        }

        private bool AddDirectoryV2(TipInfo info)
        {
            string xmlPath = DirectoryV2Path;
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            var root = xmlDoc.DocumentElement;
            
            var element = xmlDoc.CreateElement("tree");
            element.SetAttribute(PATH, info.path);
            element.SetAttribute(TITLE, info.title);
            element.SetAttribute(GUID, info.guid);
            element.SetAttribute(IS_SHOW, info.isShow.ToString());
            root.AppendChild(element);
            // TODO:将指定的节点紧接着插入指定的引用节点之后
            // root.InsertAfter(element, element);
            
            xmlDoc.Save(xmlPath);
            
            AssetDatabase.Refresh();
            return true;
        }
        
        //替换
        private bool UpdateDirectoryV2(TipInfo info)
        {
            string xmlPath = DirectoryV2Path;
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);
            
            var treeNodes = xmlDoc.SelectNodes("trees/tree");
            if (treeNodes != null)
            {
                foreach (XmlNode node in treeNodes)
                {
                    var element = node as XmlElement;
                    if (element == null)
                        continue;

                    if (element.GetAttribute(GUID) == info.guid)
                    {
                        element.SetAttribute(PATH, info.path);
                        element.SetAttribute(TITLE, info.title);
                        element.SetAttribute(IS_SHOW, info.isShow.ToString());
                        break;
                    }
                }
            }
            xmlDoc.Save(xmlPath);
            AssetDatabase.Refresh();
            return true;
        }
        
        //创建
        private void CreateGuid2TipInfo()
        {
            string xmlPath = DirectoryV2Path;
            var xmlDoc = new XmlDocument();
            xmlDoc.Load(xmlPath);

            bool isModify = false;
            var treeNodes = xmlDoc.SelectNodes("trees/tree");
            if (treeNodes != null)
            {
                _guid2TipInfo = new Dictionary<string, TipInfo>(treeNodes.Count);
                foreach (XmlNode node in treeNodes)
                {
                    var element = node as XmlElement;
                    if (element == null)
                        continue;

                    string path = element.GetAttribute(PATH);
                    string title = element.GetAttribute(TITLE);
                    string guid = element.GetAttribute(GUID);
                    bool isShow = Convert.ToBoolean(element.GetAttribute(IS_SHOW));
                    TipInfo info = new TipInfo(path, title, guid, isShow);
                    _guid2TipInfo.Add(guid, info);

                    string guid2Path = AssetDatabase.GUIDToAssetPath(guid);
                    if (path != guid2Path)
                    {
                        isModify = true;
                        element.SetAttribute(PATH, guid2Path);
                    }

                    //TODO KERVEN:Check file is exists.
                    // bool isDirectory = (File.GetAttributes(guid2Path) & FileAttributes.Directory) == FileAttributes.Directory;
                    // bool isExists = false;
                    // if (isDirectory)
                    // {
                    //     isExists = Directory.Exists(guid2Path);
                    // }
                    // else
                    // {
                    //     isExists = File.Exists(guid2Path);
                    // }
                    //
                    // if (!isExists)
                    // {
                    //     Debug.LogError($"File is delete, path = {guid2Path}");
                    // }
                }
            }

            if (isModify)
            {
                xmlDoc.Save(xmlPath);
                AssetDatabase.Refresh();
            }
        }
    }
}

