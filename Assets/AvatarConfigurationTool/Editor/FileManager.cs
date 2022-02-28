using System;
using UnityEngine;
using System.IO;
using UnityEditor;
using Newtonsoft.Json;
using Newtonsoft.Json.UnityConverters.Math;

namespace ACT.IO
{
    public class FileManager : MonoBehaviour
    {
        public static string RootPath = Application.dataPath;
        public static string SettingExtension = ".actsettings";
        public static string ProjectExtension = "actproject";
        public static string PoseExtension = "actpose";
        public static string[] FileFilters = new string[] { "ACT Project", "actProject", "All files", "*" };

        /// <summary>
        /// Generic save method
        /// </summary>
        /// <typeparam name="T">Type to save</typeparam>
        /// <param name="file">Object to save</param>
        /// <param name="filename">filename to save to</param>
        public static void Save<T>(T file, string filename)
        {
            string fullpath = filename;
            try
            {
                JsonSerializer serializer = new JsonSerializer();

                serializer.Converters.Add(new Vector3Converter());
                serializer.Converters.Add(new QuaternionConverter());
                serializer.Formatting = Formatting.Indented;
                

                using (StreamWriter sw = new StreamWriter(filename))
                using(JsonWriter writer = new JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, file);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Exception: " + e.Message);
                EditorUtility.DisplayDialog("File saving error!", "Error saving settings file", "Ok");
            }
        }
        /// <summary>
        /// Generic Load method
        /// </summary>
        /// <typeparam name="T">Type of object to load</typeparam>
        /// <param name="filename">Filename to load</param>
        /// <returns>Result if successfully loaded</returns>
        public static T Load<T>(string filename)
        {
            string filePath = filename; ;
            T file = default;
            if (File.Exists(filePath))
            {
                try
                {
                    JsonSerializer serializer = new JsonSerializer();

                    serializer.Converters.Add(new Vector3Converter());
                    serializer.Converters.Add(new QuaternionConverter());
                    serializer.Formatting = Formatting.Indented;

                    using(StreamReader sr = new StreamReader(filePath))
                    using (JsonReader reader = new JsonTextReader(sr))
                    {
                        file = serializer.Deserialize<T>(reader);
                    }
                }
                catch (Exception e)
                {
                    EditorUtility.DisplayDialog("File loading error!", "Error opening settings file, corrupt data!", "Ok");
                }
            }
            return file;
        }
        /// <summary>
        /// Save Data static method
        /// </summary>
        /// <param name="data">Data to save</param>
        /// <param name="path">Filename to save to</param>
        public static void SaveData(Data data, string path)
        {
            if(data != null)
                Save<Data>(data, path);
        }
        /// <summary>
        /// Load Data static method
        /// </summary>
        /// <param name="path">Path to load from</param>
        /// <returns>Was loading successful</returns>
        public static Data LoadData(string path)
        {            
            var data = Load<Data>(path);
            return data;
        }
    }
}