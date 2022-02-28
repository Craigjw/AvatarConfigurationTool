using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ACT.SettingsConfig;

namespace ACT
{
    public class ACTGizmos : MonoBehaviour
    {
        /// <summary>
        /// Perhaps use OnSceneGUI to draw the gizmo's instead of using a MB component.
        /// </summary>        

        ///Reference to the Editor
        public IACTEditor ACTEditor;

        /// <summary>
        /// Default world size scale
        /// </summary>
        private float worldSize = 1.0f;

        #region Interface Properties
        public bool ShowOriginalGizmos
        {
            get { return ACTEditor.ShowOriginalGizmos; }
        }
        public bool ShowStoredGizmos
        {
            get { return ACTEditor.ShowStoredGizmos; }
        }
        bool ShowPrevGizmos
        {
            get { return ACTEditor.ShowPrevGizmos; }
        }
        bool ShowHeadGizmos
        {
            get { return ACTEditor.ShowHeadGizmos; }
        }
        bool ShowAvatarSkeleton
        {
            get { return ACTEditor.ShowAvatarSkeleton; }
        }
        bool ShowModelSkeleton
        {
            get { return ACTEditor.ShowModelSkeleton; }
        }
        public bool IsAvatarInspectorActive 
        {
            get { return ACTEditor.IsAvatarInspectorActive; }
        }
        public float HandleSize
        {
            get { return ACTEditor.HandleSize; }
        }
        public Skeleton SceneSkeleton
        {
            get { return ACTEditor.Data.SceneSkeleton; }
        }
        public Skeleton AvatarSkeleton
        {
            get { return ACTEditor.Data.AvatarSkeleton; }
        }
        public void SetACTEditor(IACTEditor editorWindow)
        {
            ACTEditor = editorWindow;
        }
        #endregion
        /// <summary>
        /// Main entry method for drawing gimzos to the scene view window
        /// </summary>
        void OnDrawGizmos()
        {
            if (ACTEditor != null)
            {
                DrawGizmos();
            }
        }        
        /// <summary>
        /// Main DrawGizmos method
        /// </summary>
        private void DrawGizmos()
        {
            if(AreAnyGizmosActive())
            {
                if (IsAvatarInspectorActive)
                {
                    if (ShowAvatarSkeleton && AvatarSkeleton != null)
                    {
                        DrawBoneGizmo(AvatarSkeleton.HipBone);
                    }
                }
                else
                {                    
                    if (ShowModelSkeleton && SceneSkeleton != null)
                    {
                        DrawBoneGizmo(SceneSkeleton.HipBone);
                    }
                }
            }            
        }
        /// <summary>
        /// Are any Gizmos active to be drawn
        /// </summary>
        /// <returns>Whether we want to draw any gizmos</returns>
        bool AreAnyGizmosActive()
        {
            if (ShowAvatarSkeleton || ShowModelSkeleton || ShowStoredGizmos || ShowPrevGizmos)
                return true;
            return false;
        }
        /// <summary>
        /// Draws Gizmos for a Bone
        /// </summary>
        /// <param name="bone"></param>
        void DrawBoneGizmo(Bone bone)
        {
            if (!ShowHeadGizmos && bone.HumanName == HumanBodyBones.Head)
                return;
            if (ShowOriginalGizmos)
                DrawOriginalGeometryGizmos(bone);          
            if (ShowAvatarSkeleton || ShowModelSkeleton)
                DrawModelGeometryGizmos(bone);
                        
            foreach (var child in bone.Children)
            {
                DrawBoneGizmo(child);
            }
        }
        /// <summary>
        /// Draw Bone and Joint Gizmos for a Bone into the Scene View window
        /// </summary>
        /// <param name="bone"></param>
        void DrawModelGeometryGizmos(Bone bone)
        {
            Gizmos.color = Settings.CurrentBoneColour.Value;
            Handles.color = Settings.CurrentBoneColour.Value;
            if(bone.IsHandBone)
                DrawJoint(bone.CurrentAvatarGeometry, Settings.CurrentFingerJointSize.Value, Settings.GlobalFingerJointSize.Value);
            else
                DrawJoint(bone.CurrentAvatarGeometry, Settings.CurrentJointSize.Value, Settings.GlobalJointSize.Value);
            
            if (bone.ParentBone != null)
                DrawBone(bone.CurrentAvatarGeometry, bone.ParentBone.CurrentAvatarGeometry);
        }
        /// <summary>
        /// Draws the original Geometry.  Used for debug purposes
        /// </summary>
        /// <param name="bone">Bone to draw geometry for</param>
        void DrawOriginalGeometryGizmos(Bone bone)
        {
            Gizmos.color = Settings.DefaultBoneColour.Value;
            Handles.color = Settings.DefaultBoneColour.Value;
            DrawJoint(bone.OriginalAvatarGeometry, Settings.DefaultJointSize.Value, Settings.GlobalJointSize.Value);
            if (bone.ParentBone != null)
                DrawBone(bone.OriginalAvatarGeometry, bone.ParentBone.OriginalAvatarGeometry);
        }  
        /// <summary>
        /// Draws a bone Joint gizmo
        /// </summary>
        /// <param name="joint">AvatarTransform to draw</param>
        /// <param name="handleSize">Size modifier of the handle</param>
        /// <param name="globalSize">Global size modifier of the handle</param>
        void DrawJoint(AvatarTransform joint, float handleSize, float globalSize)
        {
            Handles.FreeMoveHandle(joint.Position, joint.Rotation, handleSize * worldSize * globalSize, Vector3.zero, Handles.SphereHandleCap);
        }
        /// <summary>
        /// Draws a bone gizmo
        /// </summary>
        /// <param name="parent">Parent to draw the bone from</param>
        /// <param name="child">Child to draw the bone to</param>
        void DrawBone(AvatarTransform parent, AvatarTransform child)
        {
            Handles.DrawLine(child.Position, parent.Position);
        }
    }
}
