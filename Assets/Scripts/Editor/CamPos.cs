//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEditor;
//using System;
////using ACT.IO;
//public class CamPos : EditorWindow
//{
//    [MenuItem("Window/CamPos")]
//    static void Init()
//    {
//        CamPos window = (CamPos)EditorWindow.GetWindow(typeof(CamPos));
//        window.Show();
//    }

//    public PositionData Pos;
//    private string filepath = "Data";

//    private void OnGUI()
//    {        
//        if (GUILayout.Button("Store Pos"))
//        {
//            StoreCamera();
//        }
//        if(GUILayout.Button("Restore Pos"))
//        {
//            RestoreCamera();
//        }
//        GUILayout.BeginHorizontal();
//        if (GUILayout.Button("Load Position"))
//        {
//            if (Pos != null)
//            {
//                string filepath = EditorUtility.OpenFilePanel("Load Camera Position", "", "camPos");
//                if (filepath != string.Empty)
//                {
//                    Pos = FileManager.Load<PositionData>(filepath);
//                    RestoreCamera();
//                }
//            }
//        }
//        if(GUILayout.Button("Save To File"))
//        {
//            if(Pos != null)
//            {
//                string filepath = EditorUtility.SaveFilePanel("Save Camera Position", "Cam", "CamPos", "camPos");
//                if(filepath != string.Empty)
//                    FileManager.Save<PositionData>(Pos, filepath);
//            }
//        }
//        GUILayout.EndHorizontal();
//    }
//    void StoreCamera()
//    {
//        var scene = SceneView.lastActiveSceneView;
//        Pos = new PositionData(scene.pivot, scene.rotation, scene.size);
//    }
//    void RestoreCamera()
//    {       
//        var scene = SceneView.lastActiveSceneView;
//        scene.pivot = Pos.Position;
//        scene.rotation = Pos.Rotation;
//        scene.size = Pos.Size;        
//        scene.Repaint();
//    }
//}
//[Serializable]
//public class PositionData
//{
//    public Vector3 Position;
//    public Quaternion Rotation;
//    public float Size;
//    public PositionData(Vector3 pos, Quaternion rot, float size)
//    {
//        Position = pos;
//        Rotation = rot;
//        Size = size;
//    }
    
//}
