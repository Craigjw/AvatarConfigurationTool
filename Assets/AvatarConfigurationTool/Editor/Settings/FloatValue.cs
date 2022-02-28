using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ACT.HelperFunctions;
using UnityEditor;

namespace ACT.SettingsConfig
{
    public struct FloatValue
    {
        public string Name { get; set; }
        float defaultValue;
        float value;
        public float Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                EditorPrefs.SetFloat(Name, value);
            }
        }
        /// <summary>
        /// Gets a FloatValue given a key name and value
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="defaultValue">Value</param>
        public FloatValue(string name, float defaultValue)
        {
            Name = name;
            this.defaultValue = defaultValue;
            if (EditorPrefs.HasKey(Name))
                value = EditorPrefs.GetFloat(Name);
            else
            {
                value = this.defaultValue;
                EditorPrefs.SetFloat(Name, value);
            }
        }
        /// <summary>
        /// Refresh the key
        /// </summary>
        public void Refresh()
        {
            if (EditorPrefs.HasKey(Name))
                value = EditorPrefs.GetFloat(Name);
            else
                EditorPrefs.SetFloat(Name, value);
        }
        /// <summary>
        /// Resets the key to default
        /// </summary>
        public void Reset()
        {
            value = defaultValue;
            EditorPrefs.SetFloat(Name, value);
        }
    }    
}