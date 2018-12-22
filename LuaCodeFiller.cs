#define USING_NGUI
//#define USING_UGUI
using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters;
using System.Xml.Serialization;

/*
 1.数据先直接使用从树形结构转化过来的列表形式，后面再处理树形结构的收放
 2.对齐放置元素。
 3.Editor下的变量在代码重新编译后，要重新初始化
 
 */

namespace com.tencent.pandora.tools
{
    public class LuaCodeFiller
    {
        private static LuaCodeFiller _instance;
        private static LuaCodeGererator _gererator;
        private const string ACTION_NAME = "ACTION_NAME";
        private Transform _actionRoot;
        private FillerElement _actionDataRoot;
        private List<FillerElement> _actionDataList = new List<FillerElement>();

        private Dictionary<Transform, FillerElement> _selected;


        //相对路径
        private const string LUA_FILE_PATH_TEMPLATE = "Actions/Resources/{0}/Lua";

        //插入点标记
        private const string PANEL_INIT_INSERT_POINT = "-- PanelInit_Insert_Point";
        private const string ADD_EVENT_LISTENNERS_INSERT_POINT = "--AddEventListeners_Insert_Point";
        private const string ON_CLICK_FUNCTION_INSERT_POINT = "--OnClickFunction_Insert_Point";

        private List<string> _luaFileNameTemplateList = new List<string>()
        {
            "{0}Controller.lua.bytes",
            "{0}Panel.lua.bytes",
        };

        //可以展示的component类型名
#if USING_NGUI
        private List<string> _componentFilter = new List<string> { "UILabel", "UIScrollView", "UIButton" };
#endif

#if USING_UGUI
        private string[] _componentFilter = { "", "", };
#endif

        public static LuaCodeFiller Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new LuaCodeFiller();
                    _gererator = new LuaCodeGererator();
                }
                _instance.SetActionRoot();
                return _instance;
            }
        }
        public string ActionName { get { return ACTION_NAME; } }

        public Transform ActionRoot { get { return _actionRoot; } set { _actionRoot = value; } }

        public List<FillerElement> DataList { get { return _actionDataList; } }
        public FillerElement DataRoot { get { return _actionDataRoot; } }

        public void SetActionRoot()
        {
            //如果_root 为空，自动赋值
            if (_actionRoot == null)
            {
                string actionName = EditorPrefs.GetString(ACTION_NAME, "");
                if (actionName != "")
                {
                    GameObject go = GameObject.Find(actionName);
                    _actionRoot = (go == null) ? null : go.transform;
                }
            }
            if (_actionRoot == null)
            {
                return;
            }
            UpdateActionData();
        }

        public void UpdateActionData()
        {
            UpdateActionDataList();
            //todo 生成树形结构
            _actionDataRoot = TreeElementUtility.ListToTree<FillerElement>(_actionDataList);

        }
        private void UpdateActionDataList()
        {
            if (_actionDataList == null)
            {
                _actionDataList = new List<FillerElement>();
            }
            _actionDataList.Clear();

            Recursive(_actionRoot, -1, _actionDataList);
        }

        private void Recursive( Transform trans, int depth, List<FillerElement> dataList )
        {
            FillerElement element = new FillerElement();
            element.CachedTransform = trans;
            element.name = trans.name;
            element.id = trans.gameObject.GetInstanceID();
            element.depth = depth;
            element.ComponentsDict = new Dictionary<string, bool>();

            List<string> componentNames = GetFilterdComponentNames(trans);
            foreach (var item in componentNames)
            {
                if (!element.ComponentsDict.ContainsKey(item))
                {
                    element.ComponentsDict.Add(item, false);
                }
            }

            dataList.Add(element);

            if (trans.childCount > 0)
            {
                depth++;
                for (int i = 0, length = trans.childCount; i < length; i++)
                {
                    Recursive(trans.GetChild(i), depth, dataList);
                }
            }

        }


        private List<string> GetFilterdComponentNames( Transform trans )
        {
            List<string> result = new List<string>();
            string pattern = @"\([^)]*\)";

            var components = trans.GetComponents<Component>();
            foreach (var item in components)
            {
                var matches = Regex.Matches(item.ToString(), pattern);
                string lastMatch = matches[matches.Count - 1].Value;
                string componentName = lastMatch.Trim('(', ')');

                if (_componentFilter.Contains(componentName))
                {
                    result.Add(componentName);
                }
            }
            return result;
        }

        public void Fill( List<FillerElement> dataList )
        {
            _gererator.GenerateLuaCode(dataList);
            WriteData();
            Dictionary<string, string> fillContent = GetFillContentDict();
            FillArea(fillContent);
        }

        private void LoadData()
        {
            //_gererator. = LuaCodeRecorder.Read(DataType.Function, LuaCodeFiller.Instance.ActionRoot.name);
            //_componentDataDict = LuaCodeRecorder.Read(DataType.Component, LuaCodeFiller.Instance.ActionRoot.name);
        }

        private void WriteData()
        {
            LuaCodeRecorder.Write(DataType.Component, ActionRoot.name, _gererator.ComponentDataDict);
            LuaCodeRecorder.Write(DataType.ButtonFunctionMap, ActionRoot.name, _gererator.ButtonFunctionMapDict);
            LuaCodeRecorder.Write(DataType.Function, ActionRoot.name, _gererator.FunctionDataDict);
        }

        private Dictionary<string, string> GetFillContentDict()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string componentContent = ConcatDictValue(_gererator.ComponentDataDict, "\r\n\t") + PANEL_INIT_INSERT_POINT;
            string buttonFunctionMapContent = ConcatDictValue(_gererator.ButtonFunctionMapDict, "\r\n\t") + ADD_EVENT_LISTENNERS_INSERT_POINT;
            string functionContent = ConcatDictValue(_gererator.FunctionDataDict, "\r\n") + ON_CLICK_FUNCTION_INSERT_POINT;

            result.Add(PANEL_INIT_INSERT_POINT, componentContent);
            result.Add(ADD_EVENT_LISTENNERS_INSERT_POINT, buttonFunctionMapContent);
            result.Add(ON_CLICK_FUNCTION_INSERT_POINT, functionContent);
            return result;
        }

        private void FillArea( Dictionary<string, string> contentDict )
        {
            for (int i = 0, length = _luaFileNameTemplateList.Count; i < length; i++)
            {
                string luaFileFolderPath = Path.Combine(Application.dataPath, string.Format(LUA_FILE_PATH_TEMPLATE, _actionRoot.name));
                string luaFileName = string.Format(_luaFileNameTemplateList[i], _actionRoot.name);
                string luaFilePath = Path.Combine(luaFileFolderPath, luaFileName);
                if (File.Exists(luaFilePath) == false)
                {
                    Logger.Log(luaFilePath + "不存在");
                    return;
                }
                string fileContent = File.ReadAllText(luaFilePath);
                var enumrator = contentDict.GetEnumerator();
                while (enumrator.MoveNext() == true)
                {
                    fileContent = Regex.Replace(fileContent, enumrator.Current.Key, enumrator.Current.Value);
                }
                File.WriteAllText(luaFilePath, fileContent);
            }
        }

        //连接字典value值
        private string ConcatDictValue( Dictionary<string, string> dict, string seperator )
        {
            StringBuilder sb = new StringBuilder(512);
            foreach (var item in dict)
            {
                sb.Append(item.Value);
                sb.Append(seperator);
            }
            return sb.ToString();
        }

        //private static string GetLuaFilePath()
        //{


        //}

        private void Insert()
        {

        }

    }


    public class FillerElement : TreeElement
    {
        [SerializeField]
        private bool _gameObjectSelected = false;
        private Dictionary<string, bool> _componentsDict;
        private Transform _cachedTransform;
        private bool _isFold = false;

        public bool GameObjectSelected
        {
            get { return _gameObjectSelected; }
            set { _gameObjectSelected = value; }
        }

        public Dictionary<string, bool> ComponentsDict
        {
            get { return _componentsDict; }
            set { _componentsDict = value; }
        }

        public Transform CachedTransform
        {
            get { return _cachedTransform; }
            set { _cachedTransform = value; }
        }

        public bool IsFold
        {
            get { return _isFold; }
            set { _isFold = value; }
        }
    }
}
