using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using ACT.HelperFunctions;

namespace ACT.SettingsConfig
{
    public static class Settings
    {
        public static ColorValue DefaultBoneColour;
        public static ColorValue CurrentBoneColour;
        public static ColorValue SavedBoneColour;
        public static FloatValue GlobalJointSize;
        public static FloatValue GlobalFingerJointSize;
        public static FloatValue DefaultJointSize;
        public static FloatValue CurrentJointSize;
        public static FloatValue SavedJointSize;
        public static FloatValue DefaultFingerJointSize;
        public static FloatValue CurrentFingerJointSize;
        public static FloatValue SavedFingerJointSize;
        public static StringValue LastProjectPath;

        /// <summary>
        /// Initializes the default values
        /// </summary>
        public static void Init()
        {
            DefaultBoneColour = new ColorValue("ACT_DefaultBoneColour", new Color(0.5f, 0.5f, 0.5f, 0.42f));
            CurrentBoneColour = new ColorValue("ACT_CurrentBoneColour", new Color(1, 0, 0, 0.42f));
            SavedBoneColour = new ColorValue("ACT_SavedBoneColour", new Color(0, 0, 1f, 0.42f));
            GlobalJointSize = new FloatValue("ACT_GlobalJointSize", 1f);
            GlobalFingerJointSize = new FloatValue("ACT_GlobalFingerJointSize", 1f);
            DefaultJointSize = new FloatValue("ACT_DefaultJointSize", 0.024f);
            CurrentJointSize = new FloatValue("ACT_CurrentJointSize", 0.024f);
            SavedJointSize = new FloatValue("ACT_SavedJointSize", 0.024f);
            DefaultFingerJointSize = new FloatValue("ACT_DefaultFingerJointSize", 0.015f);
            CurrentFingerJointSize = new FloatValue("ACT_CurrentFingerJointSize", 0.015f);
            SavedFingerJointSize = new FloatValue("ACT_SavedFingerJointSize", 0.015f);
            LastProjectPath = new StringValue("ACT_LastProjectPath", Application.dataPath);
        }
        /// <summary>
        /// Refreshes the current values
        /// </summary>
        public static void Refresh()
        {
            DefaultBoneColour.Refresh();
            CurrentBoneColour.Refresh();
            SavedBoneColour.Refresh();
            GlobalJointSize.Refresh();
            GlobalFingerJointSize.Refresh();
            DefaultJointSize.Refresh();
            CurrentJointSize.Refresh();
            SavedJointSize.Refresh();
            DefaultFingerJointSize.Refresh();
            CurrentFingerJointSize.Refresh();
            SavedFingerJointSize.Refresh();
            LastProjectPath.Refresh();
        }
        /// <summary>
        /// Resets the current values to default
        /// </summary>
        public static void Reset()
        {
            DefaultBoneColour.Reset();
            CurrentBoneColour.Reset();
            SavedBoneColour.Reset();
            GlobalJointSize.Reset();
            GlobalFingerJointSize.Reset();
            DefaultJointSize.Reset();
            CurrentJointSize.Reset();
            SavedJointSize.Reset();
            DefaultFingerJointSize.Reset();
            CurrentFingerJointSize.Reset();
            SavedFingerJointSize.Reset();
            LastProjectPath.Reset();
        }
    }        
}