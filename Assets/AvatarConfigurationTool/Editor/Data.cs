using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEditor;
using ACT.HelperFunctions;

namespace ACT
{
    [Serializable]
    public class Data
    {
        [NonSerialized] public GameObject SourceFbx;
        [NonSerialized] public GameObject SceneObject;
        [NonSerialized] public GameObject AvatarObject;

        public string SourceFbxName;
        public string SourceFbxFilename;

        public string avatarObjectName;
        public string AvatarObjectName { get { return avatarObjectName; } }
        public string sceneObjectName;
        public string SceneObjectName { get { return sceneObjectName; } }

        public string filePath;
        public string projectPath;

        public Skeleton SceneSkeleton;
        public Skeleton AvatarSkeleton;

        /// <summary>
        /// Constructor
        /// </summary>
        public Data()
        {
            SceneSkeleton = null;
            AvatarSkeleton = null;            
        }
        /// <summary>
        /// Constructor overload
        /// </summary>
        /// <param name="fbxSource">Source Fbx GameObject</param>
        public Data(GameObject fbxSource)
        {
            SourceFbx = fbxSource;
            SourceFbxName = fbxSource.name;
            SourceFbxFilename = AssetDatabase.GetAssetPath(fbxSource);
        }                
        /// <summary>
        /// Is the Data Valid?
        /// </summary>
        /// <returns>Validity</returns>
        public bool IsValid()
        {
            if (SourceFbx != null
                && (SceneObject != null || AvatarObject != null)
                && (SceneSkeleton != null || AvatarSkeleton != null))
                return true;
            return false;
        }
        /// <summary>
        /// Is the Scene Source GameObject Valid
        /// </summary>
        /// <returns>Validity</returns>
        public bool IsSceneSourceValid()
        {
            if (SceneObject != null && SceneSkeleton != null)
                return true;
            return false;
        }
        /// <summary>
        /// Is the Avatar Source GameObject Valid?
        /// </summary>
        /// <returns>Validity</returns>
        public bool IsAvatarSourceValid()
        {
            if (AvatarObject != null && SceneSkeleton != null)
                return true;
            return false;
        }
        /// <summary>
        /// Is the FbxSource Valid
        /// </summary>
        /// <returns>Validity</returns>
        public bool IsSourceValid()
        {
            if (SourceFbx != null
                && (SourceFbxFilename != null || SourceFbxFilename != string.Empty)
                && (SourceFbxName != null || SourceFbxName != string.Empty))
                return true;
            return false;
        }
        /// <summary>
        /// Configures the Avatar Skeleton used within the Avatar Configuration Window.
        /// </summary>
        /// <param name="source">Source GameObject in the Avatar Configuration Scene Window</param>
        public void ConfigureAvatarSkeleton(GameObject source)
        {
            if (source != null)
            {
                if (AvatarSkeleton == null)
                    AvatarSkeleton = AvatarMapper.ConfigureSkeleton(source);
                else
                    AvatarMapper.ReconfigureSkeleton(AvatarSkeleton, source);                    
                AvatarObject = source;
                avatarObjectName = source.name;
            }
        }
        /// <summary>
        /// Configures the Scene Skeleton used within the Generic Scene View windo
        /// </summary>
        /// <param name="source">GameObject source used within the active scene view window</param>
        public void ConfigureSceneSkeleton(GameObject source)
        {
            if (source != null)
            {
                if (SceneSkeleton == null)
                    SceneSkeleton = AvatarMapper.ConfigureSkeleton(source);
                else
                    AvatarMapper.ReconfigureSkeleton(SceneSkeleton, source);                
                SceneObject = source;
                sceneObjectName = source.name;
            }
        }        
        /// <summary>
        /// Loads the Data for when the generic scene view window is active
        /// </summary>
        /// <param name="data">Data to populate</param>
        public void LoadActiveSceneData(Data data)
        {
            try
            {                
                if (data.SceneSkeleton != null && data.SceneSkeleton.Bones.Count > 0)
                {
                    if (SceneObject != null)
                    {
                        CopyData(data);
                        AvatarMapper.LoadSkeleton(SceneSkeleton, SceneObject);
                    }
                }               
            }
            catch(Exception e)
            {
                Debug.LogError("An error was encountered while trying to load the Scene Configuration Data!");
            }
        }
        /// <summary>
        /// Loads data when the Avatar Configuration Window is active
        /// </summary>
        /// <param name="data">Data to populate</param>
        public void LoadActiveAvatarData(Data data)
        {
            try
            {
                if(data.AvatarSkeleton != null && data.AvatarSkeleton.Bones.Count > 0)
                {
                    if(AvatarObject != null)
                    {
                        CopyData(data);
                        AvatarMapper.LoadSkeleton(AvatarSkeleton, AvatarObject);
                    }
                }
            }
            catch(Exception e)
            {
                Debug.LogError("An error was encountered while trying to load the Avatar Configuration Data!");
            }
        }
        /// <summary>
        /// Loads data when the Avatar Configuration Window is not open.
        /// </summary>
        /// <param name="data">Data to populate</param>
        public void LoadInactiveAvatarData(Data data)
        {
            try
            {
                if (data.AvatarSkeleton != null && data.AvatarSkeleton.Bones.Count > 0)
                {
                    CopyData(data);
                    AvatarMapper.LoadInactiveSkeleton(AvatarSkeleton);
                }
            }
            catch(Exception e)
            {
                Debug.LogError("An error was encountered while trying to load the Avatar Configuration Data!");
            }
        }
        /// <summary>
        /// Loads data when the generic scene view window is not open, i.e. Avatar Configuration window is open
        /// </summary>
        /// <param name="data">Data to populate</param>
        public void LoadInactiveSceneData(Data data)
        {
            try
            {
                if (data.SceneSkeleton != null && data.SceneSkeleton.Bones.Count > 0)
                {
                    CopyData(data);
                    AvatarMapper.LoadInactiveSkeleton(SceneSkeleton);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("An error was encountered while trying to load the Avatar Configuration Data!");
            }
        }
        /// <summary>
        /// Copy method
        /// </summary>
        /// <param name="source">Source to copy</param>
        private void CopyData(Data source)
        {
            this.avatarObjectName = source.avatarObjectName;
            this.AvatarSkeleton = source.AvatarSkeleton;
            this.filePath = source.filePath;
            this.projectPath = source.filePath;
            this.sceneObjectName = source.sceneObjectName;
            this.SceneSkeleton = source.SceneSkeleton;
            this.SourceFbxFilename = source.SourceFbxFilename;
            this.SourceFbxName = source.SourceFbxName;
        }
        /// <summary>
        /// Compare method, compares the data against an Fbx GameObject (Asset) in order to validate if the data is configured to the same asset
        /// </summary>
        /// <param name="fbxModel">Source GameObject asset to compare against</param>
        /// <returns>Result</returns>
        public bool CompareModel(GameObject fbxModel)
        {
            if(fbxModel != null)
            {
                var path = AssetDatabase.GetAssetPath(fbxModel);
                if (path != string.Empty
                    && SourceFbxName == fbxModel.name
                    && SourceFbxFilename == path)
                    return true;                
            }
            return false;            
        }    
    }
}