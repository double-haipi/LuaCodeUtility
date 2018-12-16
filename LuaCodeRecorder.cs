/*
 1.保持记录唯一性
 */

using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using com.tencent.pandora.MiniJSON;


namespace com.tencent.pandora.tools
{
    public enum DataType
    {
        Function,
        Component,
    }
    public class LuaCodeRecorder
    {
        private static string _relativeParentPath = "Editor/LuaCodeUtility/Datas";
        private static string _functionDataFileName = "{0}_functionData";
        private static string _componentDataFileName = "{0}_componentData";

        public static Dictionary<string, string> Read(DataType type, string actionName)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            string path = GetDataFilePath(type, actionName);
            string content = "";
            Dictionary<string, object> deserialized = new Dictionary<string, object>();
            if (File.Exists(path))
            {
                content = File.ReadAllText(path);
            }

            if (string.IsNullOrEmpty(content) == false)
            {
                deserialized = Json.Deserialize(content) as Dictionary<string, object>;
            }

            if (deserialized != null && deserialized.Count > 0)
            {
                foreach (var item in deserialized)
                {
                    string value = item.Value as string;
                    if (value != null)
                    {
                        result.Add(item.Key, value);
                    }
                }
            }
            return result;
        }


        public static void Write(DataType type, string actionName, Dictionary<string, string> data)
        {
            if (data == null || data.Count == 0)
            {
                return;
            }
            string content = Json.Serialize(data);
            string path = GetDataFilePath(type, actionName);
            File.WriteAllText(path, content);
        }



        private static string GetDataFilePath(DataType type, string actionName)
        {

            string folderPath = Path.Combine(Application.dataPath, _relativeParentPath);
            string dataFileName;
            switch (type)
            {
                case DataType.Function:
                    dataFileName = string.Format(_functionDataFileName, actionName);
                    break;
                case DataType.Component:
                    dataFileName = string.Format(_componentDataFileName, actionName);
                    break;
                default:
                    dataFileName = "";
                    break;
            }

            if (string.IsNullOrEmpty(dataFileName))
            {
                return "";
            }

            return Path.Combine(folderPath, dataFileName);
        }
    }
}