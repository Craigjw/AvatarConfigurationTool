using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ACT.HelperFunctions;
using UnityEditor;

namespace ACT.SettingsConfig
{
    public struct StringValue
    {
        public string Name { get; set; }

        string defaultValue;
        string value;
        public string Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                EditorPrefs.SetString(Name, value);
            }
        }
        /// <summary>
        /// Gets a StringValue given a key name and value
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="defaultValue">Value</param>
        public StringValue(string name, string defaultValue)
        {
            Name = name;
            this.defaultValue = defaultValue;
            if (EditorPrefs.HasKey(Name))
                value = EditorPrefs.GetString(Name);
            else
            {
                value = this.defaultValue;
                EditorPrefs.SetString(Name, value);
            }
        }
        /// <summary>
        /// Refresh the key
        /// </summary>
        public void Refresh()
        {
            if (EditorPrefs.HasKey(Name))
                value = EditorPrefs.GetString(Name);
            else
                EditorPrefs.SetString(Name, value);
        }
        /// <summary>
        /// Resets the key to default
        /// </summary>
        public void Reset()
        {
            value = defaultValue;
            EditorPrefs.SetString(Name, value);
        }
    }
}