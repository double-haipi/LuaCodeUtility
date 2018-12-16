/*
 1.保持记录唯一性
 */

using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using com.tencent.pandora.MiniJSON;
using System.Runtime.Serialization.Formatters;
using System.Xml.Serialization;

namespace com.tencent.pandora.tools
{
    //函数,组件,按钮函数对
    public enum DataType
    {
        Function,
        Component,
        ButtonFunctionMap,
    }

    //读取和写入信息
    public class LuaCodeRecorder
    {
        //函数信息
        //组件信息
        //变量名要改为const类型
        private static string _relativeParentPath = "Pandora/Editor/LuaCodeUtility/Datas";
        private static string _functionDataFileName = "{0}_functionData.json";
        private static string _componentDataFileName = "{0}_componentData.json";
        private static string _buttonFunctionMapDataFileName = "{0}_buttonFunctionMapData.json";

        //返回值: 字典,key:函数名或组件名,value:函数实现或组件的获取语句.
        public static Dictionary<string, string> Read( DataType type, string actionName )
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


        public static void Write( DataType type, string actionName, Dictionary<string, string> data )
        {
            if (data == null || data.Count == 0)
            {
                return;
            }
            string content = Json.Serialize(data);
            string path = GetDataFilePath(type, actionName);
            File.WriteAllText(path, content);
        }


        void XMLSerialize<T>( T obj, string path )
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            Stream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite);
            xs.Serialize(fs, obj);
            fs.Flush();
            fs.Close();
            fs.Dispose();
        }

        //反序列化
        T XMLDeserialize<T>( string path )
        {
            XmlSerializer xs = new XmlSerializer(typeof(T));
            Stream fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite);
            T serTest = (T)xs.Deserialize(fs);
            fs.Flush();
            fs.Close();
            fs.Dispose();
            return serTest;
        }

        private static string GetDataFilePath( DataType type, string actionName )
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
                case DataType.ButtonFunctionMap:
                    dataFileName = string.Format(_buttonFunctionMapDataFileName, actionName);
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