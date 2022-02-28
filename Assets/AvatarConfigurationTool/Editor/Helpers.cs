using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using ACT.IO;

namespace ACT.HelperFunctions
{
    public static class Helpers
    {
        /// <summary>
        /// Checks if the Avatar Configuration Window is active or not
        /// </summary>
        /// <returns>True if Avatar Configuration Window is active</returns>
        public static bool CheckAvatarInspectorActive()
        {
#if UNITY_2020_1_OR_NEWER
            Stage currentStage = StageUtility.GetCurrentStage();
            if (currentStage != null && !string.IsNullOrEmpty(currentStage.assetPath))
                return currentStage.GetType().Name == "AvatarConfigurationStage";
            return false;
#else
            var go = GetAvatarInspectorObject();
            if (go != null)
                return true;
            return false;
#endif
        }
        /// <summary>
        /// Saves a pose to file
        /// </summary>
        /// <param name="skeleton">Skeleton to save</param>
        /// <param name="filepath">Filepath to save to</param>
        public static void SavePose(Skeleton skeleton, string filepath)
        {
            int length = filepath.LastIndexOf("/");
            string path = filepath.Substring(0, length);
            if(skeleton != null && filepath != string.Empty && Directory.Exists(path))
            {
                Pose pose = new Pose(skeleton);
                FileManager.Save<Pose>(pose, filepath);                
            }            
        }
        /// <summary>
        /// Loads a pose from file
        /// </summary>
        /// <param name="filepath">filepath to load</param>
        /// <returns>Result of loading the file</returns>
        public static Pose LoadPose(string filepath)
        {
            if(filepath != string.Empty && File.Exists(filepath))
            {
                Pose pose = FileManager.Load<Pose>(filepath);
                return pose;
            }
            return null;
        }
        /// <summary>
        /// Sets the scene as dirty for repaint
        /// </summary>
        public static void SetSceneAsDirty()
        {
            var scene = EditorSceneManager.GetActiveScene();
            EditorSceneManager.MarkSceneDirty(scene);
        }
        /// <summary>
        /// Validates the Avatar Object used in the Avatar Configuration Window
        /// </summary>
        /// <param name="target">Target GameObject to validate</param>
        /// <returns>Success of the validation</returns>
        public static bool ValidateAvatarObject(GameObject target)
        {
            if (target.name.Contains("(Clone)"))
                return true;
            return false;
        }
        /// <summary>
        /// Validates the Scene object used in the generic scene view window, i.e. not the avatar configuration window.
        /// </summary>
        /// <param name="target">Target GameObject to validate</param>
        /// <returns>Success of the validation</returns>
        public static bool ValidateSceneGameObject(GameObject target)
        {
            if (ValidateHumanoid(target))
            {
                var go = SearchSceneObjects(target);
                if (go != null && go.Count > 0)
                {
                    foreach (var item in go)
                    {
                        var targetId = target.GetInstanceID();
                        var itemId = item.GetInstanceID();
                        if (targetId == itemId)
                            return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// Validates a prefab asset
        /// </summary>
        /// <param name="target">target GameObject to validate</param>
        /// <returns>Success of the validation</returns>
        public static bool ValidatePrefab(GameObject target)
        {
            if (PrefabUtility.IsPartOfPrefabAsset(target))
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Validate whether the target is a humanoid GameObject
        /// </summary>
        /// <param name="target">GameObject target of validation</param>
        /// <returns>Success of the validation</returns>
        public static bool ValidateHumanoid(GameObject target)
        {
            if (target != null)
            {
                var animator = target.GetComponent<Animator>();
                if (animator != null && animator.isHuman)
                    return true;
            }
            return false;
        }        
        /// <summary>
        /// Returns an Fbx GameObject asset from the given path
        /// </summary>
        /// <param name="path">path of the asset</param>
        /// <returns>GameObject loaded from the AssetDatabase</returns>
        public static GameObject GetFbxObject(string path)
        {
            var index = Application.dataPath.LastIndexOf("/");
            var fullPath = Application.dataPath.Substring(0, index + 1) + path;

            if(path != string.Empty && File.Exists(fullPath))
            {
                var fbx = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                return fbx;
            }
            return null;
        }
        public static string FindFbxPath(string name)
        {
            if(name.Contains("/"))
            {
                var index = name.LastIndexOf("/");
                name = name.Substring(index + 1);
                index = name.LastIndexOf(".");
                name = name.Substring(0, index);
            }
            var guids = AssetDatabase.FindAssets("t:gameobject " + name);

            if (guids != null & guids.Length > 0)
            {                
                var path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return path;
            }
            return string.Empty;
        }
        /// <summary>
        /// Gets the Avatar Inspector GameObject in the current active scene
        /// </summary>
        /// <returns></returns>
        public static GameObject GetAvatarInspectorObject()
        {
            GameObject[] allObjects = Object.FindObjectsOfType<GameObject>();

            foreach (var go in allObjects)
            {
                if (go.activeInHierarchy)
                {
                    var animator = go.GetComponentInChildren<Animator>();
                    if (animator != null && go.name.Contains("(Clone)"))
                        return go;
                }
            }
            return null;
        }
        /// <summary>
        /// Gets the original Prefab which the GameObject is sourced from
        /// </summary>
        /// <param name="target">GameObject which to find the prefab from</param>
        /// <returns>Original prefab source</returns>
        public static GameObject GetOriginalFbx(GameObject target)
        {            
            var original = GetOriginalMeshPrefab(target);
            if (original != target)
            {
                return original;
            }
            return target;
        }
        /// <summary>
        /// Finds the GameObject in the scene given a source GameObject
        /// </summary>
        /// <param name="fbxSource">GameObject source which to find in the scene</param>
        /// <returns>Scene GameObject</returns>
        public static GameObject FindSceneObject(GameObject fbxSource)
        {
            List<GameObject> results = new List<GameObject>();
            int bestCompare = 999;
            GameObject bestMatch = null;
            var go = SearchSceneObjects(fbxSource);
            if (go != null && go.Count > 0)
            {
                foreach (var item in go)
                {
                    var component = item.GetComponent<ACTGizmos>();
                    if (component != null)
                    {
                        return item;
                    }
                    results.Add(item);
                }
            }
            foreach (var item in results)
            {
                int delta = Mathf.Abs(item.name.CompareTo(fbxSource.name));
                if (delta < bestCompare)
                {
                    bestCompare = delta;
                    bestMatch = item;
                }
            }
            return bestMatch;
        }
        /// <summary>
        /// Searches for the Avatar source GameObject within the Avatar Configuration Inspector
        /// </summary>
        /// <param name="source">GameObject source to find</param>
        /// <returns>Avatar GameObject used in the Avatar Configuration Inspector</returns>
        public static GameObject SearchAvatarObject(GameObject source)
        {
            if (source != null)
            {                
                var sceneObjects = Object.FindObjectsOfType<GameObject>().ToList();
                //Do Explicit search
                var result = sceneObjects.Find(x => x.name == source.name);
                //Do Non-explicit search
                if (result == null)
                {
                    result = sceneObjects.Find(x => x.name.Contains(source.name));
                }
                var root = GetRootObject(result);
                return root;
            }
            return null;
        }
        /// <summary>
        /// Search Scene Objects for a GameObject
        /// </summary>
        /// <param name="source">GameObject to search for</param>
        /// <returns>GameObjec that we are searching for</returns>
        public static List<GameObject> SearchSceneObjects(GameObject source)
        {
            if (source != null)
            {
                List<GameObject> rootObjects = new List<GameObject>();
                GameObject[] allObjects = UnityEngine.Object.FindObjectsOfType<GameObject>();
                List<GameObject> results = new List<GameObject>();
                int count = 0;
                foreach (var go in allObjects)
                {
                    count++;
                    if (go.activeInHierarchy)
                    {
                        var root = GetRootObject(go);
                        if (!rootObjects.Contains(root))
                        {
                            rootObjects.Add(root);
                        }
                    }
                }
                foreach (var root in rootObjects)
                {
                    var assetRoot = GetOriginalMeshPrefab(root);
                    if (assetRoot != null && source.name.Contains(assetRoot.name))
                    {
                        if (!results.Contains(root))
                        {
                            if (ValidateHumanoid(root))
                                results.Add(root);
                        }
                    }
                }
                return results;
            }
            return null;
        }
        /// <summary>
        /// Gets the Original Mesh Prefab of the GameObject
        /// </summary>
        /// <param name="source">Source that we use to search for</param>
        /// <returns>Original Mesh Prefab</returns>
        public static GameObject GetOriginalMeshPrefab(GameObject source)
        {
            var smr = source.GetComponentInChildren<SkinnedMeshRenderer>();
            if (smr != null)
            {
                var mesh = smr.sharedMesh;
                if (mesh == null)
                    return null;
                var original = PrefabUtility.GetCorrespondingObjectFromOriginalSource(mesh);
                var path = AssetDatabase.GetAssetPath(original);
                return (GameObject)AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            }
            return null;
        }
        /// <summary>
        /// Gets the Root GameObject
        /// </summary>
        /// <param name="source">Source GameObject to find the root of</param>
        /// <returns>Root GameObject</returns>
        public static GameObject GetRootObject(GameObject source)
        {
            if (source == null)
                return null;
            var parent = RecurseRootTransform(source.transform);
            return parent.gameObject;
        }
        /// <summary>
        /// Recurse Root transforms
        /// </summary>
        /// <param name="t">Transform to recurse</param>
        /// <returns>Recursive Transform parent</returns>
        private static Transform RecurseRootTransform(Transform t)
        {
            if (t.parent != null)
                return RecurseRootTransform(t.parent);
            else
                return t;
        }
        /// <summary>
        /// Drag and Drop GUI box
        /// </summary>
        /// <typeparam name="T">Type of Object we are using</typeparam>
        /// <param name="obj">Object that is being used</param>
        /// <param name="label">Text label of the Drag & Drop box</param>
        /// <returns>Returned Object being Drag and Dropped</returns>
        public static T DropAreaGUI<T>(T obj, string label)
        {
            Event evt = Event.current;
            Rect drop_area = GUILayoutUtility.GetRect(0.0f, 50.0f, GUILayout.ExpandWidth(true));
            GUI.Box(drop_area, label);

            switch (evt.type)
            {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!drop_area.Contains(evt.mousePosition))
                        return obj;

                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();
                        foreach (Object dragged_object in DragAndDrop.objectReferences)
                        {
                            if (dragged_object != null && dragged_object.GetType() == typeof(T))
                            {
                                GUI.changed = true;
                                var castObject = (T)System.Convert.ChangeType(dragged_object, typeof(T));
                                GUI.changed = true;
                                return castObject;
                            }
                        }
                    }
                    break;
            }
            return obj;
        }
        /// <summary>
        /// Draws a line in the GUI
        /// </summary>
        /// <param name="color">Color</param>
        /// <param name="thickness">Thickness in pixels</param>
        /// <param name="padding">Padding in pixels</param>
        public static void DrawUILine(Color color, int thickness = 2, int padding = 10)
        {
            Rect r = EditorGUILayout.GetControlRect(GUILayout.Height(padding + thickness));
            r.height = thickness;
            r.y += padding / 2;
            r.x -= 2;
            r.width += 6;
            EditorGUI.DrawRect(r, color);
        }
        /// <summary>
        /// Draws a Box in the GUI
        /// </summary>
        /// <param name="borderColor">Border Color</param>
        /// <param name="backgroundColor">Background Color</param>
        /// <param name="rect">Rect coordinates to draw the box</param>
        /// <param name="width">Width in pixels of the box lines</param>
        public static void DrawUIBox(Color borderColor, Color backgroundColor, Rect rect, int width = 2)
        {
            Rect outter = new Rect(rect);
            Rect inner = new Rect(rect.x + width, rect.y + width, rect.width - width * 2, rect.height - width * 2);
            EditorGUI.DrawRect(outter, borderColor);
            EditorGUI.DrawRect(inner, backgroundColor);
        }
        /// <summary>
        /// Clears the Console for debug.
        /// </summary>
        public static void ClearConsole()
        {
            //var assembly = Assembly.GetAssembly(typeof(SceneView));
            var assembly = System.Reflection.Assembly.GetAssembly(typeof(UnityEditor.Editor));
            var type = assembly.GetType("UnityEditor.LogEntries");
            var method = type.GetMethod("Clear");
            method.Invoke(new object(), null);
        }
        /// <summary>
        /// Resizes a texture, as icons with a native size are drawn with lower quality than resizing them
        /// </summary>
        /// <param name="texture2D">Texture to resize</param>
        /// <param name="targetX">Target X dimension</param>
        /// <param name="targetY">Target Y dimension</param>
        /// <returns>Resized Texture2D</returns>
        public static Texture2D ResizeTexture(Texture2D texture2D, int targetX, int targetY)
        {
            RenderTexture rt = new RenderTexture(targetX, targetY, 24);
            RenderTexture.active = rt;
            Graphics.Blit(texture2D, rt);
            Texture2D result = new Texture2D(targetX, targetY);
            result.ReadPixels(new Rect(0, 0, targetX, targetY), 0, 0);
            result.Apply();
            return result;
        }
        /// <summary>
        /// Converts a Color to a String representation
        /// </summary>
        /// <param name="colour">Color to convert</param>
        /// <returns>String representation of a Color</returns>
        public static string ColourToString(Color colour)
        {
            string value = ColorUtility.ToHtmlStringRGBA(colour);
            return value;
        }
        /// <summary>
        /// Converts a string representation of a color to a Color value
        /// </summary>
        /// <param name="value">String representation of a Color</param>
        /// <returns>Color value</returns>
        public static Color StringToColour(string value)
        {
            Color newColour = Color.red;
            ColorUtility.TryParseHtmlString("#" + value, out newColour);
            return newColour;
        }
    }
}