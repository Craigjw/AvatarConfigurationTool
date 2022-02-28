using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ACT.HelperFunctions;
using UnityEditor;

namespace ACT.SettingsConfig
{
    public struct ColorValue
    {
        public string Name { get; set; }

        Color defaultValue;
        Color value;
        public Color Value
        {
            get
            {
                return value;
            }
            set
            {
                this.value = value;
                EditorPrefs.SetString(Name, Helpers.ColourToString(value));
            }
        }
        /// <summary>
        /// Gets a ColorValue given a key name and value
        /// </summary>
        /// <param name="name">Name of the key</param>
        /// <param name="defaultValue">Value</param>
        public ColorValue(string name, Color defaultValue)
        {
            Name = name;
            this.defaultValue = defaultValue;
            if (EditorPrefs.HasKey(Name))
                value = Helpers.StringToColour(EditorPrefs.GetString(Name));
            else
            {
                value = this.defaultValue;
                EditorPrefs.SetString(Name, Helpers.ColourToString(value));
            }
        }
        /// <summary>
        /// Refreshes the key
        /// </summary>
        public void Refresh()
        {
            if (EditorPrefs.HasKey(Name))
                value = Helpers.StringToColour(EditorPrefs.GetString(Name));
            else
                EditorPrefs.SetString(Name, Helpers.ColourToString(value));
        }
        /// <summary>
        /// Resets the key to default
        /// </summary>
        public void Reset()
        {
            value = defaultValue;
            EditorPrefs.SetString(Name, Helpers.ColourToString(value));
        }
    }
}
