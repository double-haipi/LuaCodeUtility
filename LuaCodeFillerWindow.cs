using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Text;
using System.Collections.Generic;

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
            DrawDataList();
            DrawButtons();
            EditorGUILayout.EndVertical();
        }

        private void DrawActionPanelField()
        {
            _filler.ActionRoot = (Transform)EditorGUILayout.ObjectField("ActionPanel：", _filler.ActionRoot, typeof(Transform), true);
            if (_filler.ActionRoot != null && _filler.ActionRoot.name != EditorPrefs.GetString(_filler.ActionName))
            {
                EditorPrefs.SetString(_filler.ActionName, _filler.ActionRoot.name);
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

            }

            if (GUILayout.Button("Execute", GUILayout.Width(80f)))
            {

            }
            EditorGUILayout.EndHorizontal();
        }


        #endregion


    }
}