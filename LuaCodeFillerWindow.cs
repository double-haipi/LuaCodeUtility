using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace com.tencent.pandora.tools
{
    public class LuaCodeFillerWindow : EditorWindow
    {
        private LuaCodeFiller _filler;
        private Vector2 _horizontalScrollPosition = Vector2.zero;
        private Vector2 _verticalScrollPosition = Vector2.zero;


        #region GUI
        [MenuItem("GameObject/LuaCodeFiller", priority = 11)]
        private static void Init()
        {
            EditorWindow.GetWindow(typeof(LuaCodeFillerWindow), false, "LuaCodeFiller", true).Show();
        }

        private void OnEnable()
        {
            //Selection.selectionChanged = OnSelectionChanged;
            if (_filler == null)
            {
                _filler = LuaCodeFiller.Instance;
            }
        }


        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            DrawActionPanelField();
            DrawFoldOrUnfold();
            DrawDataList();
            DrawButtons();
            EditorGUILayout.EndVertical();
        }

        private bool _isFold = false;
        private void DrawFoldOrUnfold()
        {
            string iconDesc = "";
            if (_isFold)
            {
                iconDesc = "\u25BA";
            }
            else
            {
                iconDesc = "\u25BC";
            }
            _isFold = GUILayout.Toggle(_isFold, iconDesc, "PreToolbar2");
        }
        private void DrawActionPanelField()
        {
            _filler.ActionRoot = (Transform)EditorGUILayout.ObjectField("ActionPanel：", _filler.ActionRoot, typeof(Transform), true);
            if (_filler.ActionRoot != null && _filler.ActionRoot.name != EditorPrefs.GetString(_filler.ActionName))
            {
                EditorPrefs.SetString(_filler.ActionName, _filler.ActionRoot.name);
                _filler.SetActionRoot();
            }
        }

        private void DrawSettingArea(bool hasBoxCollider, ref string variableName, ref string bindFunctionName)
        {
            EditorGUILayout.TextField("变量名：", variableName, GUILayout.Width(50f));
            if (hasBoxCollider == true)
            {
                EditorGUILayout.TextField("响应函数名：", variableName, GUILayout.Width(50f));
            }
        }

        private void DrawDataList()
        {
            GUILayout.Space(5f);
            _verticalScrollPosition = EditorGUILayout.BeginScrollView(_verticalScrollPosition);
            _horizontalScrollPosition = EditorGUILayout.BeginScrollView(_horizontalScrollPosition);
            List<FillerElement> dataList = _filler.DataList;
            for (int i = 0, length = dataList.Count; i < length; i++)
            {
                DrawElement(dataList[i]);
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.EndScrollView();
        }

        private void DrawElement(FillerElement element)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.Space((element.depth + 2) * 18);
            EditorGUILayout.LabelField(element.name);

            if (element.name != _filler.ActionRoot.name)
            {
                element.GameObjectSelected = EditorGUILayout.Toggle("self", element.GameObjectSelected);

                Dictionary<string, bool> componentsDict = element.ComponentsDict;
                List<string> keys = new List<string>(componentsDict.Keys);
                for (int i = 0, length = keys.Count; i < length; i++)
                {
                    componentsDict[keys[i]] = EditorGUILayout.Toggle(keys[i], componentsDict[keys[i]]);
                }
            }

            EditorGUILayout.EndHorizontal();
        }
        private void DrawButtons()
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Refresh", GUILayout.Width(80f)))
            {
                _filler.UpdateActionData();
            }

            if (GUILayout.Button("Execute", GUILayout.Width(80f)))
            {
                //复制一份数据,否则数据变化会导致无法使用foreach遍历
                List<FillerElement> newDataList = new List<FillerElement>();
                foreach (var item in _filler.DataList)
                {
                    FillerElement element = new FillerElement();
                    element.name = item.name;
                    element.id = item.id;
                    element.depth = item.depth;
                    element.CachedTransform = item.CachedTransform;
                    element.GameObjectSelected = item.GameObjectSelected;
                    Dictionary<string, bool> componentDict = new Dictionary<string, bool>();
                    foreach (var innerItem in item.ComponentsDict)
                    {
                        componentDict.Add(innerItem.Key, innerItem.Value);
                    }
                    element.ComponentsDict = componentDict;
                    newDataList.Add(element);
                }
                _filler.Fill(newDataList);
            }
            EditorGUILayout.EndHorizontal();
        }


        #endregion


    }
}
