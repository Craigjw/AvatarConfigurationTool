using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Linq;
using ACT.UndoRedo;
using UnityEditor.SceneManagement;
using ACT.HelperFunctions;
using ACT.IO;
using ACT.SettingsConfig;

namespace ACT
{
    public class AvatarConfigurationEditor : EditorWindow, IACTEditor
    {
        #region Variable declarations
        private ACTGizmos avatarGizmos;
        static string editorPath = string.Empty;

        Vector2 projectScrollRect;
        Vector2 presetScrollRect;
        Vector2 settingsScrollRect;

        Color backgroundColour;
        Color lineColour;

        GameObject sceneObject;
        GameObject dropSource;
        GameObject sceneSource;
        GameObject fbxSource;

        bool isProjectLocked = false;    
        bool hasChanged = false;
        bool doUndoRedo = false;

        float lastInvokeTime;
        float lastChangeTime;        
        float lightMaxWidth = 180f;
        float lightMinWidth = 120f;
        float poseButtonMaxheight = 50;
        float poseButtonMaxWidth = 50;

        readonly float interval = 0.02f;
        readonly float maxInterval = 0.2f;
        readonly Vector2Int toolbarButtonSize = new Vector2Int(32, 32);
        readonly Vector2Int lightSize = new Vector2Int(32, 32);

        string[] tabStrings = { "Project", "Options" };
        int tabOption = 0;

        GUIContent[] toolbarContent;        
        int toolbarOption = -1;        

        Texture2D inactiveLight;        
        Texture2D activeLight;
        Texture2D offLight;
        Texture2D errorLight;
        Texture2D newProject;
        Texture2D loadProject;
        Texture2D saveProject;
        Texture2D saveAsProject;
        Texture2D undo;
        Texture2D redo;
        Texture2D saveAvatarPose;
        Texture2D loadAvatarPose;
        Texture2D resetAvatarPose;
        Texture2D setAvatarPose;

        GUIContent sceneActiveButton;
        GUIContent sceneInactiveButton;
        GUIContent sceneOffButton;
        GUIContent sceneErrorButton;
        GUIContent avatarActiveButton;
        GUIContent avatarInactiveButton;
        GUIContent avatarOffButton;
        GUIContent avatarErrorButton;
        GUIContent newProjectButton;
        GUIContent loadProjectButton;
        GUIContent saveProjectButton;
        GUIContent saveAsProjectButton;
        GUIContent undoButton;
        GUIContent redoButton;
        GUIContent saveAvatarPoseButton;
        GUIContent loadAvatarPoseButton;
        GUIContent resetAvatarPoseButton;
        GUIContent setAvatarPoseButton;

        GUIStyle errorStyle;
        GUIStyle normalStyle;
        GUIStyle titleStyle;
        GUIStyle buttonStyle;
        GUIStyle toolButtonStyle;
        GUISkin toolbarSkin;

        #region Properties
        public bool ShowGizmos { get; set; } = true;
        public bool ShowOriginalGizmos { get; set; }
        public bool ShowStoredGizmos { get; set; }
        public bool ShowPrevGizmos { get; set; }
        public bool ShowHeadGizmos { get; set; }
        public bool ShowAvatarSkeleton { get; set; } = true;
        public bool ShowModelSkeleton { get; set; } = true;
        public bool IsAvatarInspectorActive { get; set; }
        public float HandleSize { get; set; }
        public Data Data { get; set; }

        GameObject SceneObject
        {
            get { return sceneObject; }
            set
            {
                SetSceneSource(value);
            }
        }
        GUIStyle ErrorStyle
        {
            get { return GetErrorStyle(); }
        }
        GUIStyle NormalStyle
        {
            get { return GetNormalStyle(); }
        }
        GUIStyle TitleStyle
        {
            get { return GetTitleStyle(); }
        }
        GUIStyle ButtonStyle
        {
            get { return GetButtonStyle(); }
        }
        GUIStyle ToolbarButtonStyle
        {
            get { return GetToolbarButtonStyle(); }
        }
        #endregion
        #endregion

        [MenuItem("Window/ACT")]
        static void Init()
        {
            editorPath = GetBaseDirectory();
            var window = EditorWindow.GetWindow<AvatarConfigurationEditor>();
            window.Show();
        }        
        void OnEnable()
        {
            editorPath = GetBaseDirectory();
            Settings.Init();
            InitSkin();
            InitButtons();
            InitToolbarButtons();
            if (Data == null)
                Data = new Data();
            ResetUpdateTimer();            
        }
        void OnDestroy()
        {
            RemoveACTComponent();
        }
        void OnFocus()
        {
            Settings.Refresh();
        }
#if UNITY_EDITOR        
        void Update()
        {            
            if (!EditorApplication.isPlaying)
            {
                if (Time.realtimeSinceStartup - lastInvokeTime > interval)
                {
                    lastInvokeTime = Time.realtimeSinceStartup;
                    if (CheckForChange())
                    {
                        hasChanged = true;
                        lastChangeTime = lastInvokeTime;
                    }
                    UpdateGeometry();
                }
                if (hasChanged)
                {
                    if (Time.realtimeSinceStartup - lastChangeTime > maxInterval)
                    {                        
                        if (!doUndoRedo)
                        {
                            StorePrevArmature();
                            DoCmd();
                            SceneView.RepaintAll();
                            hasChanged = false;
                        }
                        ResetChangeFlags();
                    }
                }
            }
        }
#endif   
        /// <summary>
        /// Checks whether we have changed editor windows within Unity
        /// </summary>
        private void OnInspectorUpdate()
        {            
            if (Helpers.CheckAvatarInspectorActive())
            {
                if (IsAvatarInspectorActive == false)
                {
                    ActivateAvatarSkeleton();                    
                    IsAvatarInspectorActive = true;
                    Repaint();
                }
            }
            else
            {
                if (IsAvatarInspectorActive == true)
                {
                    ActivateSceneSkeleton();                    
                    IsAvatarInspectorActive = false;
                    Repaint();
                }
            }
        }
        private void OnGUI()
        {
            Rect rect = new Rect(6, 6, position.width - 12, position.height - 12);
            Rect borderRect = new Rect(2, 2, position.width - 4, position.height - 4);
            GUI.Box(borderRect, GUIContent.none);
            Helpers.DrawUIBox(lineColour, backgroundColour, borderRect, 2);
            GUILayout.BeginArea(rect);
            ToolbarGUI();
            TabGUI();
            ActiveObjectGUI();
            HelpTextGUI();
            switch(tabOption)
            {
                case 0:
                    ProjectGUI();
                    break;                
                case 1:
                    OptionsGUI();
                    break;                
            }
            GUILayout.EndArea();
            SceneView.RepaintAll();
        }
        /// <summary>
        /// Draws the Tabs GUI
        /// </summary>
        private void TabGUI()
        {             
            EditorGUILayout.BeginHorizontal();
            tabOption = GUILayout.Toolbar(tabOption, tabStrings);
            EditorGUILayout.EndHorizontal();
            Helpers.DrawUILine(lineColour, 2, 0);
        }
        /// <summary>
        /// Draws the Toolbar GUI
        /// </summary>
        private void ToolbarGUI()
        {
            EditorGUILayout.BeginHorizontal();
            int totalWidth = toolbarContent.Length * toolbarButtonSize.x;
            toolbarOption = GUILayout.Toolbar(toolbarOption, toolbarContent, ToolbarButtonStyle, GUILayout.Width(totalWidth), GUILayout.Height(toolbarButtonSize.y));

            EditorGUILayout.EndHorizontal();

            Helpers.DrawUILine(lineColour, 2, 0);

            switch(toolbarOption)
            {
                case 0: //new project
                    toolbarOption = -1;
                    NewProject();
                    break;
                case 1: //load project
                    toolbarOption = -1;
                    LoadProject();
                    break;
                case 2: //save project
                    toolbarOption = -1;
                    SaveProject();
                    break;
                case 3: //save as project
                    toolbarOption = -1;
                    SaveProjectAs();
                    break;
                case 4: //undo
                    toolbarOption = -1;
                    UndoCmd();
                    break;
                case 5: //redo
                    toolbarOption = -1;
                    ReDoCmd();
                    break;
            }
            toolbarOption = -1;            
        }
        /// <summary>
        /// Draws the Active Object GUI
        /// </summary>
        private void ActiveObjectGUI()
        {    
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (!IsAvatarInspectorActive)
            {
                if (ShowModelSkeleton)
                    ShowActiveSceneButtons();
                else
                    ShowInactiveSceneButtons();
            }
            else
            {
                if (ShowAvatarSkeleton)
                    ShowActiveAvatarButtons();
                else
                    ShowInactiveAvatarButtons();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        /// <summary>
        /// Shows the Active Scene Buttons
        /// </summary>
        private void ShowActiveSceneButtons()
        {
            if (Data.SceneObject == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(sceneErrorButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));
                EditorGUI.EndDisabledGroup();
            }
            else if (GUILayout.Button(sceneActiveButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y)))
            {
                ShowModelSkeleton = !ShowModelSkeleton;
            }

            if (Data.AvatarSkeleton == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(avatarErrorButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));
                EditorGUI.EndDisabledGroup();
            }
            else
                GUILayout.Button(avatarInactiveButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));
        }
        /// <summary>
        /// Shows the Inactive Scene buttons
        /// </summary>
        private void ShowInactiveSceneButtons()
        {
            if (Data.SceneObject == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(sceneErrorButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));
                EditorGUI.EndDisabledGroup();
            }
            else if (GUILayout.Button(sceneOffButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y)))
            {
                ShowModelSkeleton = !ShowModelSkeleton;
            }
            if (Data.AvatarSkeleton == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(avatarErrorButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));
                EditorGUI.EndDisabledGroup();
            }
            else
                GUILayout.Button(avatarInactiveButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));
        }
        /// <summary>
        /// Shows the Active Avatar Buttons
        /// </summary>
        private void ShowActiveAvatarButtons()
        {
            if (Data.SceneObject == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(sceneErrorButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));
                EditorGUI.EndDisabledGroup();
            }
            else
                GUILayout.Button(sceneInactiveButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));

            if (Data.AvatarObject == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(avatarErrorButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));
                EditorGUI.EndDisabledGroup();
            }
            else if (GUILayout.Button(avatarActiveButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y)))
            {
                ShowAvatarSkeleton = !ShowAvatarSkeleton;
            }
        }
        /// <summary>
        /// Shows the Inactive Avatar Buttons
        /// </summary>
        private void ShowInactiveAvatarButtons()
        {
            if (Data.SceneObject == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(sceneErrorButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));
                EditorGUI.EndDisabledGroup();
            }
            else
                GUILayout.Button(sceneInactiveButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));

            if (Data.AvatarObject == null)
            {
                EditorGUI.BeginDisabledGroup(true);
                GUILayout.Button(avatarErrorButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y));
                EditorGUI.EndDisabledGroup();
            }
            else if (GUILayout.Button(avatarOffButton, ButtonStyle, GUILayout.MinWidth(lightMinWidth), GUILayout.MaxWidth(lightMaxWidth), GUILayout.Height(lightSize.y)))
            {
                ShowAvatarSkeleton = !ShowAvatarSkeleton;
            }
        }
        /// <summary>
        /// Displays the simple Help Text GUI
        /// </summary>
        private void HelpTextGUI()
        {
            EditorGUILayout.HelpBox("Select a model from the Scene to begin.", MessageType.Info, true);
            GUILayout.Space(5);
        }
        /// <summary>
        /// Draws the Project Tab GUI
        /// </summary>
        private void ProjectGUI()
        {
            projectScrollRect = EditorGUILayout.BeginScrollView(projectScrollRect);            
            Helpers.DrawUILine(lineColour, 2, 0);
            EditorGUILayout.BeginHorizontal();

            EditorGUI.BeginDisabledGroup(isProjectLocked);
            EditorGUI.BeginChangeCheck();
            dropSource = Helpers.DropAreaGUI<GameObject>(dropSource, "Drag & Drop Model here");
            if (EditorGUI.EndChangeCheck())
            {
                if (ConfigureDropSource(dropSource))
                    ConfigureProject();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();            
            Helpers.DrawUILine(lineColour, 2, 0);
            GUILayout.Space(5);
            EditorGUI.BeginDisabledGroup(isProjectLocked);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            var style = GetLabelStyle(ValidateFbxSource());
            EditorGUILayout.LabelField("Fbx Model", style, GUILayout.Width(110));
            fbxSource = (GameObject)EditorGUILayout.ObjectField(fbxSource, typeof(GameObject), false);
            EditorGUILayout.EndHorizontal();
            if (EditorGUI.EndChangeCheck())
            {
                if (ConfigureDropSource(fbxSource))
                    ConfigureProject();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(!isProjectLocked);
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.BeginHorizontal();
            style = GetLabelStyle(ValidateSceneSource());
            EditorGUILayout.LabelField("Scene Object", style, GUILayout.Width(110));
            sceneSource = (GameObject)EditorGUILayout.ObjectField(sceneSource, typeof(GameObject), true);
            EditorGUILayout.EndHorizontal();            
            if (EditorGUI.EndChangeCheck())
            {
                ConfigureSceneSource(sceneSource);
            }
            EditorGUI.EndDisabledGroup();
            GUILayout.Space(5);
            Helpers.DrawUILine(lineColour, 2, 0);
            GUILayout.Space(5);

            EditorGUI.BeginDisabledGroup(!isProjectLocked);
            PoseButtons();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndScrollView();
        }
        /// <summary>
        /// Draws Pose Buttons
        /// </summary>
        private void PoseButtons()
        {
            EditorGUILayout.BeginHorizontal();
            //GUILayout.FlexibleSpace();            
            if (GUILayout.Button(loadAvatarPoseButton, ButtonStyle,
                GUILayout.Width(poseButtonMaxWidth),
                GUILayout.Height(poseButtonMaxheight)))
            {
                if (IsAvatarInspectorActive)
                    LoadPose(Data.AvatarSkeleton);
                else
                    LoadPose(Data.SceneSkeleton);
            }
            if (GUILayout.Button(saveAvatarPoseButton, ButtonStyle,
                GUILayout.Width(poseButtonMaxWidth),
                GUILayout.Height(poseButtonMaxheight)))
            {
                if (IsAvatarInspectorActive)
                    SavePose(Data.AvatarSkeleton);
                else
                    SavePose(Data.SceneSkeleton);
            }
            GUILayout.Space(10);
            if (GUILayout.Button(resetAvatarPoseButton, ButtonStyle, 
                GUILayout.Width(poseButtonMaxWidth),
                GUILayout.Height(poseButtonMaxheight)))
            {
                ResetPose();
            }
            if (GUILayout.Button(setAvatarPoseButton, ButtonStyle,
                GUILayout.Width(poseButtonMaxWidth),
                GUILayout.Height(poseButtonMaxheight)))
            {
                SetDefaultPose();
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();
        }
        /// <summary>
        /// Draws the Presets GUI, WIP, so not used at present
        /// </summary>
        private void PresetsGUI()
        {
            presetScrollRect = EditorGUILayout.BeginScrollView(presetScrollRect);

            EditorGUILayout.EndScrollView();
        }
        /// <summary>
        /// Draws the Options Tab GUI
        /// </summary>
        private void OptionsGUI()
        {
            Helpers.DrawUILine(lineColour, 2, 0);
            settingsScrollRect = EditorGUILayout.BeginScrollView(settingsScrollRect);
            GUILayout.Label("Global Pose Settings", TitleStyle);

            EditorGUI.BeginChangeCheck();
            Settings.GlobalJointSize.Value = EditorGUILayout.Slider("Joint Size", Settings.GlobalJointSize.Value, 0, 10);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            Settings.GlobalFingerJointSize.Value = EditorGUILayout.Slider("Finger Joint Size", Settings.GlobalFingerJointSize.Value, 0, 10);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            GUILayout.Space(5);
            Helpers.DrawUILine(lineColour, 2, 0);
            GUILayout.Space(5);
            
            Helpers.DrawUILine(lineColour, 2, 0);
            GUILayout.Space(5);

            GUILayout.Label("Character Pose Settings", TitleStyle);
            EditorGUI.BeginChangeCheck();
            Settings.CurrentBoneColour.Value = EditorGUILayout.ColorField("Bone Color", Settings.CurrentBoneColour.Value);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            Settings.CurrentJointSize.Value = EditorGUILayout.Slider("Joint Size", Settings.CurrentJointSize.Value, 0, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {                
                SceneView.RepaintAll();
            }

            EditorGUI.BeginChangeCheck();
            Settings.CurrentFingerJointSize.Value = EditorGUILayout.Slider("Finger Joint Size", Settings.CurrentFingerJointSize.Value, 0, 0.1f);
            if (EditorGUI.EndChangeCheck())
            {
                SceneView.RepaintAll();
            }
            GUILayout.Space(5);
            
            GUILayout.Space(5);
            Helpers.DrawUILine(lineColour, 2, 0);
            GUILayout.Space(5);

            EditorGUILayout.EndScrollView();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset", GUILayout.Width(90), GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Reset Options",
                    "Are you sure you want to reset the Options?",
                    "Reset",
                    "Cancel"))
                    Settings.Reset();
            }
            GUILayout.EndHorizontal();
        }
        /// <summary>
        /// Sets up a New Project
        /// </summary>
        private void NewProject()
        {
            RemoveACTComponent();
            Data = new Data();
            isProjectLocked = false;
            hasChanged = false;
            dropSource = null;
            fbxSource = null;
            sceneSource = null;
            avatarGizmos = null;
            hasChanged = false;
            doUndoRedo = false;
            //Helpers.ClearConsole();
            ResetUpdateTimer();
        }
        /// <summary>
        /// Loads a project
        /// </summary>
        private void LoadProject()
        {
            string filepath = EditorUtility.OpenFilePanelWithFilters("Load project", Settings.LastProjectPath.Value, FileManager.FileFilters);
            if (filepath != string.Empty)
            {
                var data = FileManager.Load<Data>(filepath);
                if (data != null)
                {
                    if (fbxSource == null)
                    {
                        Settings.LastProjectPath.Value = filepath;
                        LoadProjectData(data);
                        isProjectLocked = true;
                    }
                    else if (data.SourceFbxFilename == Data.SourceFbxFilename)
                    {
                        Settings.LastProjectPath.Value = filepath;
                        LoadProjectData(data);
                        isProjectLocked = true;
                    }
                    else
                    {
                        if(EditorUtility.DisplayDialog("Invalid ACT Project Selected",
                            "The project selected is for a different model, unable to load the Project!",
                            "Load Project",
                            "Cancel"))
                            
                        {
                            Settings.LastProjectPath.Value = filepath;
                            LoadProjectData(data);
                            isProjectLocked = true;
                        }
                    }
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid file selected",
                        "An invalid project file was selected, please select a different file",
                        "Ok");
                }
            }
            else
            {
                EditorUtility.DisplayDialog("Invalid file selected",
                        "An invalid project file was selected, please select a different file",
                        "Ok");
            }            
        }
        /// <summary>
        /// Loads Project Data
        /// </summary>
        /// <param name="data"></param>
        private void LoadProjectData(Data data)
        {
            var fbx = Helpers.GetFbxObject(data.SourceFbxFilename);
            if(fbx == null)
            {
                var path = Helpers.FindFbxPath(data.SourceFbxFilename);
                if (path != null && path != string.Empty)
                    data.SourceFbxFilename = path;
                fbx = Helpers.GetFbxObject(data.SourceFbxFilename);
            }
            if (fbx != null)
            {
                Data.SourceFbx = fbx;                
                Data.SourceFbxFilename = data.SourceFbxFilename;
                Data.SourceFbxName = data.SourceFbxName;
                fbxSource = fbx;
                var sceneObject = Helpers.FindSceneObject(fbx);
                if (IsAvatarInspectorActive)
                {
                    Data.AvatarObject = sceneObject;
                    SceneObject = sceneObject;
                    Data.LoadActiveAvatarData(data);
                    Data.LoadInactiveSceneData(data);
                }
                else
                {
                    Data.SceneObject = sceneObject;                    
                    SceneObject = sceneObject;
                    Data.LoadActiveSceneData(data);
                    Data.LoadInactiveAvatarData(data);
                }

            }
            else
            {
                EditorUtility.DisplayDialog("Error Loading project file",
                    "The FBX Model file that the project specifies can't be located within the current Unity Project.  Has it been moved, renamed or removed?",
                    "Ok");
            }
        }
        /// <summary>
        /// Saves a project
        /// </summary>
        private void SaveProject()
        {            
            if(Data != null && Data.SourceFbx != null && (Data.SceneObject != null || Data.AvatarObject != null))
            {
                if(Data.AvatarSkeleton != null || Data.SceneSkeleton != null)
                {
                    if (Data.projectPath != null && Data.projectPath != string.Empty && Data.projectPath.Contains(Application.dataPath))
                    {
                        FileManager.SaveData(Data, Data.projectPath);
                    }
                    else
                    {
                        SaveProjectAs();
                    }
                }
            }
        }
        /// <summary>
        /// Saves a project to a named file
        /// </summary>
        private void SaveProjectAs()
        {            
            if (Data != null && Data.SourceFbx != null && (Data.SceneObject != null || Data.AvatarObject != null))
            {
                if (Data.AvatarSkeleton != null || Data.SceneSkeleton != null)
                {
                    int index = Settings.LastProjectPath.Value.LastIndexOf("/") + 1;
                    var filename = Settings.LastProjectPath.Value.Substring(index);
                    string filepath = EditorUtility.SaveFilePanel("Save ACT Project", Settings.LastProjectPath.Value, filename, FileManager.ProjectExtension);
                    if (filepath != string.Empty && filepath.Contains(Application.dataPath))
                    {
                        Data.projectPath = filepath;
                        Settings.LastProjectPath.Value = filepath;
                        FileManager.SaveData(Data, filepath);
                    }
                }
            }
        }
        /// <summary>
        /// Saves the current Pose to file
        /// </summary>
        /// <param name="skeleton"></param>
        private void SavePose(Skeleton skeleton)
        {            
            if(skeleton != null)
            {
                string filepath = EditorUtility.SaveFilePanel("Save ACT Pose", Data.filePath, "ACTPose", FileManager.PoseExtension);                                
                if (filepath != string.Empty && filepath.Contains(Application.dataPath))
                {
                    var length = filepath.LastIndexOf("/");
                    Data.filePath = filepath.Substring(0, length);
                    Helpers.SavePose(skeleton, filepath);
                }
                else
                {
                    EditorUtility.DisplayDialog("Invalid path selected",
                        "Please select a valid save path within Unity Assets directory",
                        "Ok");
                }
            }
        }
        /// <summary>
        /// Loads a pose from file
        /// </summary>
        /// <param name="skeleton"></param>
        private void LoadPose(Skeleton skeleton)
        {
            if(skeleton != null)
            {
                string filepath = EditorUtility.OpenFilePanel("Load ACT Pose", Data.filePath, FileManager.PoseExtension);
                if(filepath != string.Empty)
                {
                    var length = filepath.LastIndexOf("/");
                    Data.filePath = filepath.Substring(0, length);
                    var pose = Helpers.LoadPose(filepath);
                    skeleton.ApplyPose(pose);
                    SceneView.RepaintAll();
                }
            }
        }
        /// <summary>
        /// Resets a pose to the default
        /// </summary>
        private void ResetPose()
        {
            if (EditorUtility.DisplayDialog("Reset Pose to Default", "Are you sure you want to Reset the current pose to the default?", "Ok", "Cancel"))
            {
                if (Data != null)
                {
                    if (IsAvatarInspectorActive)
                    {
                        if (Data.AvatarSkeleton != null)
                        {
                            Data.AvatarSkeleton.ResetGeometry();
                            Helpers.SetSceneAsDirty();
                            var selectedObjects = Selection.objects;
                            Selection.objects = Data.AvatarSkeleton.GetAllObjects();
                            SceneView.RepaintAll();
                            Selection.objects = selectedObjects;
                            SceneView.RepaintAll();
                        }
                    }
                    else
                    {
                        if (Data.SceneSkeleton != null)
                        {
                            Data.SceneSkeleton.ResetGeometry();
                            Helpers.SetSceneAsDirty();
                            var selectedObjects = Selection.objects;
                            Selection.objects = Data.SceneSkeleton.GetAllObjects();
                            SceneView.RepaintAll();
                            Selection.objects = selectedObjects;
                            SceneView.RepaintAll();
                        }
                    }
                    StageUtility.GoToMainStage();
                }
            }
        }
        /// <summary>
        /// Sets a default pose
        /// </summary>
        private void SetDefaultPose()
        {
            if (EditorUtility.DisplayDialog("Set Default Pose", "Are you sure you want to Set the Default pose to current?  You will lose the default pose permanently!", "Ok", "Cancel"))
            { 
                if (Data != null)
                {
                    if (IsAvatarInspectorActive)
                    {
                        if (Data.AvatarSkeleton != null)
                        {
                            Data.AvatarSkeleton.SetOriginalGeometry();
                            Helpers.SetSceneAsDirty();
                            var selectedObjects = Selection.objects;
                            Selection.objects = Data.SceneSkeleton.GetAllObjects();
                            SceneView.RepaintAll();
                            Selection.objects = selectedObjects;
                            SceneView.RepaintAll();
                        }
                    }
                    else
                    {
                        if (Data.SceneSkeleton != null)
                        {
                            Data.SceneSkeleton.SetOriginalGeometry();
                            Helpers.SetSceneAsDirty();
                            var selectedObjects = Selection.objects;
                            Selection.objects = Data.SceneSkeleton.GetAllObjects();
                            SceneView.RepaintAll();
                            Selection.objects = selectedObjects;
                            SceneView.RepaintAll();
                        }
                    }
                    StageUtility.GoToMainStage();
                }
            }
        }
        /// <summary>
        /// Do Command for Undo/Redo
        /// </summary>
        private void DoCmd()
        {
            if(IsAvatarInspectorActive)
            {
                if(Data != null && Data.AvatarSkeleton != null)
                {
                    Data.AvatarSkeleton.DoCmd();
                    Helpers.SetSceneAsDirty();
                }
            }
            else
            {
                if (Data != null && Data.SceneSkeleton != null)
                {
                    Data.SceneSkeleton.DoCmd();
                    Helpers.SetSceneAsDirty();
                }
            }
            ResetUpdateTimer();
        }
        /// <summary>
        /// Undo Command
        /// </summary>
        private void UndoCmd()
        {
            if(IsAvatarInspectorActive)
            {
                if (Data != null && Data.AvatarSkeleton != null)
                {
                    Data.AvatarSkeleton.UndoCmd();
                    Helpers.SetSceneAsDirty();
                }
            }
            else
            {
                if (Data != null && Data.SceneSkeleton != null)
                {
                    Data.SceneSkeleton.UndoCmd();
                    Helpers.SetSceneAsDirty();
                }
            }
            ResetUpdateTimer();
        }
        /// <summary>
        /// Redo Command
        /// </summary>
        private void ReDoCmd()
        {
            if (IsAvatarInspectorActive)
            {
                if (Data != null && Data.AvatarSkeleton != null)
                {
                    Data.AvatarSkeleton.Redo();
                    Helpers.SetSceneAsDirty();
                }
            }
            else
            {
                if (Data != null && Data.SceneSkeleton != null)
                {
                    Data.SceneSkeleton.Redo();
                    Helpers.SetSceneAsDirty();
                }
            }
            ResetUpdateTimer();
        }
        /// <summary>
        /// Activates the Avatar Skeleton for use with the Avatar Configuration
        /// </summary>
        private void ActivateAvatarSkeleton()
        {
            if(fbxSource != null)
            {                
                var sourceObject = Helpers.GetAvatarInspectorObject();
                if(Helpers.ValidateSceneGameObject(sourceObject))
                {
                    SceneObject = sourceObject;
                    if(Data != null)
                    {
                        Data.ConfigureAvatarSkeleton(SceneObject);
                    }
                    AddACTComponent(SceneObject);
                    ResetUpdateTimer();
                }
            }
        }
        /// <summary>
        /// Activates the Scene Skeleton for use within the active Scene view
        /// </summary>
        private void ActivateSceneSkeleton()
        {
            if (fbxSource != null)
            {
                var sourceObject = Helpers.FindSceneObject(fbxSource);
                if (sourceObject != null && Helpers.ValidateSceneGameObject(sourceObject))
                {
                    SceneObject = sourceObject;
                    if(Data != null)
                    {
                        doUndoRedo = false;
                        Data.ConfigureSceneSkeleton(SceneObject);
                    }
                    AddACTComponent(SceneObject);
                    ResetUpdateTimer();
                }
            }
        }
        /// <summary>
        /// Configures a project
        /// </summary>
        private void ConfigureProject()
        {
            isProjectLocked = true;
            Data = new Data(fbxSource);
            if (!IsAvatarInspectorActive)
                Data.ConfigureSceneSkeleton(sceneObject);
            else
                Data.ConfigureAvatarSkeleton(sceneObject);
        }
        /// <summary>
        /// Configures a Source from the Drag & Drop interface
        /// </summary>
        /// <param name="source">GameObject that is being targeted</param>
        /// <returns>If the method was successful or not</returns>
        private bool ConfigureDropSource(GameObject source)
        {
            if (Helpers.ValidateHumanoid(source))
            {
                if (Helpers.ValidatePrefab(source))
                {
                    var fbx = Helpers.GetOriginalFbx(source);
                    fbxSource = fbx;                
                    var sourceObject = Helpers.FindSceneObject(fbx);
                    if (Helpers.ValidateSceneGameObject(sceneObject))
                    {
                        SceneObject = sourceObject; 
                    }
                    ResetUpdateTimer();
                    return true;
                }
                else
                {
                    if (ConfigureSceneObject(source))
                        return true;
                    else
                    {
                        DisplayInvalidFbxSourceDialog();
                        return false;
                    }
                }
            }
            else
            {
                DisplayInvalidFbxSourceDialog();
            }
            return false;
        }
        /// <summary>
        /// Configures the Scene Object Source, relating to the Avatar Configuration
        /// </summary>
        /// <param name="source">GameObject source for configuration</param>
        /// <returns>Whether the method was successful</returns>
        private bool ConfigureSceneObject(GameObject source)
        {
            if (Helpers.ValidateSceneGameObject(source))
            {
                var prefab = Helpers.GetOriginalMeshPrefab(source);
                if (prefab != null && prefab != source)
                {
                    SceneObject = source;
                    var name = source.name;
                    if (name.Contains("(Clone)"))
                        name = name.Remove(name.Length - 7);                    
                    fbxSource = prefab;
                    Helpers.SetSceneAsDirty();
                    ResetUpdateTimer();
                    return true;
                }
            }
            return false;
        }
        /// <summary>
        /// Configure the Scene Source Game Object relating to an active Scene view
        /// </summary>
        /// <param name="source">GameObject source for configuration</param>
        /// <returns>Whether the method was successful</returns>
        private bool ConfigureSceneSource(GameObject source)
        {
            if (source != null)
            {
                if (Helpers.ValidateSceneGameObject(source))
                {
                    var fbx = Helpers.GetOriginalMeshPrefab(source);
                    if (fbx == fbxSource)
                    {
                        SceneObject = source;  
                        return true;
                    }
                    else
                    {
                        DisplayInvalidFbxSourceDialog();
                        SceneObject = SceneObject;
                    }
                }
                else
                {
                    DisplayInvalidFbxSourceDialog();
                    sceneSource = SceneObject;
                }
            }
            else
            {
                DisplayInvalidFbxSourceDialog();
                sceneSource = SceneObject;
            }
            return false;
        }
        /// <summary>
        /// Sets the Scene view Source
        /// </summary>
        /// <param name="source"></param>
        private void SetSceneSource(GameObject source)
        {
            if (SceneObject != null)
            {
                var component = SceneObject.GetComponent<ACTGizmos>();
                if (component != null)
                    DestroyImmediate(component);
            }
            sceneObject = source;
            sceneSource = source;
            //Data.SceneObject = source;
            AddACTComponent(source);
        }        
        /// <summary>
        /// Checks the Active scene for changes in the character geometry
        /// </summary>
        /// <returns>Whether their were changes made to the characters geometry</returns>
        private bool CheckForChange()
        {
            if (SceneObject != null)
            {
                if (IsAvatarInspectorActive)
                {
                    if (Data != null && Data.AvatarSkeleton != null)                        
                    {
                        if (Data.AvatarSkeleton.CheckGeometry())
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    if (Data != null && Data.SceneSkeleton != null)
                    {
                        if(Data.SceneSkeleton.CheckGeometry())                        
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Update the skeleton Geometry from the Character's Transforms
        /// </summary>
        private void UpdateGeometry()
        {
            if (SceneObject != null)
            {
                if (IsAvatarInspectorActive)
                {
                    if (Data != null && Data.AvatarSkeleton != null)
                    {
                        Data.AvatarSkeleton.UpdateGeometry();
                    }
                }
                else
                {
                    if (Data != null && Data.SceneSkeleton != null)
                    {
                        Data.SceneSkeleton.UpdateGeometry();
                    }
                }
            }
        }
        /// <summary>
        /// Reset change flags used to detect changes over a period of time
        /// </summary>
        private void ResetChangeFlags()
        {
            doUndoRedo = false;
            if (Data != null)
            {
                if (IsAvatarInspectorActive)
                {
                    if (Data.AvatarSkeleton != null)
                        Data.AvatarSkeleton.ResetChangeFlag();
                }
                else
                {
                    if (Data.SceneSkeleton != null)
                        Data.SceneSkeleton.ResetChangeFlag();
                }
            }
        }
        /// <summary>
        /// Stores the previous character geometry used to detect changes
        /// </summary>
        void StorePrevArmature()
        {
            if (IsAvatarInspectorActive)
            {
                if (Data.AvatarSkeleton != null)
                {
                    Data.AvatarSkeleton.StepGeometry();
                }
            }
            else
            {
                if (Data.SceneSkeleton != null)
                {
                    Data.SceneSkeleton.StepGeometry();
                }
            }
        }
        /// <summary>
        /// Adds the ACT component to the source object so that Gizmo's can be drawn to it
        /// </summary>
        /// <param name="source"></param>
        private void AddACTComponent(GameObject source)
        {
            if (SceneObject != null)
            {
                avatarGizmos = SceneObject.GetComponent<ACTGizmos>();
                if (avatarGizmos == null)
                {
                    avatarGizmos = SceneObject.AddComponent<ACTGizmos>();
                }
                avatarGizmos.SetACTEditor(this);
            }
        }
        /// <summary>
        /// Removes the ACT component when we are finished using ACT.
        /// </summary>
        private void RemoveACTComponent()
        {
            if (avatarGizmos != null)
            {
                if (avatarGizmos.gameObject != null)
                {
                    var gizmoComponent = avatarGizmos.gameObject.GetComponent<ACTGizmos>();
                    if (gizmoComponent != null)
                        DestroyImmediate(gizmoComponent);
                }                
            }
        }        
        /// <summary>
        /// Displays Invalid Source FBX dialog
        /// </summary>
        private void DisplayInvalidFbxSourceDialog()
        {
            EditorUtility.DisplayDialog("Invalid GameObject selected",
                "Please select a valid GameObject",
                "Ok");
        }
        /// <summary>
        /// Resets the timer used to detect changes
        /// </summary>
        void ResetUpdateTimer()
        {
            lastInvokeTime = Time.realtimeSinceStartup;
            lastChangeTime = lastInvokeTime;
        }             
        /// <summary>
        /// Validates and FBX source
        /// </summary>
        /// <returns></returns>
        bool ValidateFbxSource()
        {
            if(Data != null)
            {
                return Data.IsSourceValid();
            }
            return false;
        }
        /// <summary>
        /// Validates an Active Scene's source
        /// </summary>
        /// <returns></returns>
        bool ValidateSceneSource()
        {
            if (Data != null)
            {
                if (IsAvatarInspectorActive)                
                    return Data.IsAvatarSourceValid();                
                else
                    return Data.IsSceneSourceValid();                
            }
            return false;
        }
        /// <summary>
        /// Dynamically gets a button style
        /// </summary>
        /// <returns></returns>
        private GUIStyle GetButtonStyle()
        {
            if (buttonStyle == null)
            {
                buttonStyle = new GUIStyle(GUI.skin.button);
                buttonStyle.wordWrap = true;
            }
            return buttonStyle;
        }
        /// <summary>
        /// Dynamically gets a toolbar button style
        /// </summary>
        /// <returns></returns>
        private GUIStyle GetToolbarButtonStyle()
        {
            if(toolButtonStyle == null)
            {
                toolButtonStyle = toolbarSkin.GetStyle("menuButton");
                toolButtonStyle.fixedWidth = 100f;
            }
            return toolButtonStyle;
        }
        /// <summary>
        /// Dynamically gets an Error style
        /// </summary>
        /// <returns></returns>
        private GUIStyle GetErrorStyle()
        {
            if(normalStyle == null)
            {
                errorStyle = new GUIStyle();
                errorStyle.normal.textColor = Color.red;
                errorStyle.fontStyle = FontStyle.Bold;
            }
            return errorStyle;
        }
        /// <summary>
        /// Dynamically gets a Normal style
        /// </summary>
        /// <returns></returns>
        private GUIStyle GetNormalStyle()
        {
            if(normalStyle == null)
            {
                normalStyle = new GUIStyle();
                normalStyle.normal.textColor = Color.black;
                normalStyle.fontStyle = FontStyle.Normal;
            }
            return normalStyle;
        }
        /// <summary>
        /// Dynamically gets a Title style
        /// </summary>
        /// <returns></returns>
        private GUIStyle GetTitleStyle()
        {
            if(titleStyle == null)
            {
                titleStyle = new GUIStyle();
                titleStyle.normal.textColor = Color.black;
                titleStyle.fontStyle = FontStyle.Bold;
                titleStyle.fontSize = 12;
            }
            return titleStyle;
        }
        /// <summary>
        /// Initialize Buttons with images
        /// </summary>
        private void InitButtons()
        {
            activeLight = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/ActiveLight.png");
            inactiveLight = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/InactiveLight.png");
            offLight = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/OffLight.png");
            errorLight = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/ErrorLight.png");

            newProject = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/newProject.png");
            loadProject = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/Load.png");
            saveProject = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/Save.png");
            saveAsProject = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/SaveAs.png");
            undo = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/Undo.png");
            redo = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/Redo.png");

            saveAvatarPose = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/SaveAvatarPose.png");
            loadAvatarPose = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/LoadAvatarPose.png");
            resetAvatarPose = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/ResetAvatarPose.png");
            setAvatarPose = AssetDatabase.LoadAssetAtPath<Texture2D>(editorPath + "Images/SetAvatarPose.png");

            sceneActiveButton = new GUIContent("Scene Configuration", activeLight, "Active Scene Configuration");
            sceneInactiveButton = new GUIContent("Scene Configuration", inactiveLight, "Inactive Scene Configuration");
            sceneOffButton = new GUIContent("Scene Configuration", offLight, "Scene Configuration Off");
            sceneErrorButton = new GUIContent("Scene Configuration", errorLight, "Scene Configuration Error");

            avatarActiveButton = new GUIContent("Avatar Configuration", activeLight, "Active Avatar Configuration");
            avatarInactiveButton = new GUIContent("Avatar Configuration", inactiveLight, "Inactive Avatar Configuration");
            avatarOffButton = new GUIContent("Avatar Configuration", offLight, "Avatar Configuration Off");
            avatarErrorButton = new GUIContent("Avatar Configuration", errorLight, "Avatar Configuration Off");

            saveAvatarPoseButton = new GUIContent(saveAvatarPose, "Save Avatar Pose to file");
            loadAvatarPoseButton = new GUIContent(loadAvatarPose, "Load Avatar Pose from file");
            resetAvatarPoseButton = new GUIContent(resetAvatarPose, "Reset Avatar Pose to default");
            setAvatarPoseButton = new GUIContent(setAvatarPose, "Reset Default Avatar Pose to current Pose");

            newProjectButton = new GUIContent(newProject, "New Project");            
            loadProjectButton = new GUIContent(loadProject, "Load Project");
            saveProjectButton = new GUIContent(saveProject, "Save All");
            saveAsProjectButton = new GUIContent(saveAsProject, "Save As");
            undoButton = new GUIContent(undo, "Undo Action");
            redoButton = new GUIContent(redo, "Redo Action");            
        }
        /// <summary>
        /// Initialize Toolbar buttons
        /// </summary>
        private void InitToolbarButtons()
        {
            toolbarSkin = AssetDatabase.LoadAssetAtPath<GUISkin>(editorPath + "Editor/MenuBarSkin.guiskin");
            toolButtonStyle = toolbarSkin.GetStyle("menuButton");
            
            var content = new List<GUIContent>();
            content.Add(newProjectButton);
            content.Add(loadProjectButton);
            content.Add(saveProjectButton);
            content.Add(saveAsProjectButton);
            content.Add(undoButton);
            content.Add(redoButton);   
            toolbarContent = content.ToArray();
        }        
        /// <summary>
        /// Initializes the GUI Skin used for various content
        /// </summary>
        private void InitSkin()
        {
            if (EditorGUIUtility.isProSkin)
            {
                backgroundColour = new Color32(56, 56, 56, 255);                
                lineColour = new Color32(25, 25, 25, 255);
            }
            else
            {
                backgroundColour = new Color32(194, 194, 194, 255);
                lineColour = Color.grey;
            }
        }
        /// <summary>
        /// Dynamically gets a Label style
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private GUIStyle GetLabelStyle(bool value)
        {
            if (value)
                return NormalStyle;
            else
                return ErrorStyle;
        }       
        private static string GetBaseDirectory()
        {
            string path = string.Empty;
            var guids = AssetDatabase.FindAssets("ACTDirectoryLocator");
            if (guids != null && guids.Length > 0)
            {
                path = AssetDatabase.GUIDToAssetPath(guids[0]);
                var index = path.LastIndexOf("/");
                path = path.Substring(0, index + 1);
            }
            return path;
        }
    }
}