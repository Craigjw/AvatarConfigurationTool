using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


namespace ACT
{
    /// <summary>
    /// Bone data structure
    /// </summary>
    [Serializable]
    public class Bone
    {
        public string ModelName;
        public HumanBodyBones HumanName;
        public string ParentBoneModelName;        
        [NonSerialized] public bool HasChanged;      

        public bool IsHumanBone;
        public bool IsSkeletal;
        public bool IsRoot;
        
        public bool IsHandBone;
        public AvatarTransform OriginalAvatarGeometry;

        [NonSerialized] public AvatarTransform DynamicGeometry;
        [NonSerialized] public AvatarTransform PrevAvatarGeometry;
        [NonSerialized] public AvatarTransform CurrentAvatarGeometry;

        [NonSerialized] public Transform Parent;
        [NonSerialized] public Transform Transform;

        [NonSerialized] public Bone ParentBone;
        [NonSerialized] public List<Bone> Children;

        /// <summary>
        /// Constructor
        /// </summary>
        public Bone()
        {
            ModelName = string.Empty;
            HumanName = HumanBodyBones.LastBone;
            ParentBoneModelName = string.Empty;
            HasChanged = false;
            IsHumanBone = false;
            IsSkeletal = false;
            IsRoot = false;
            IsHandBone = false;

            Parent = null;
            Transform = null;
            ParentBone = null;

            DynamicGeometry = new AvatarTransform();
            PrevAvatarGeometry = new AvatarTransform();
            CurrentAvatarGeometry = new AvatarTransform();
            OriginalAvatarGeometry = new AvatarTransform();

            Children = new List<Bone>();
        }        
        /// <summary>
        /// Apply a pose to the character
        /// </summary>
        /// <param name="poseBone">Pose bone to apply</param>
        public void ApplyPoseBone(PoseBone poseBone)
        {
            ApplyGeometry(poseBone.Geometry);
        }
        /// <summary>
        /// When the Geometry is moved, update the dynamic geometry data
        /// </summary>
        public void UpdateGeometry()
        {
            DynamicGeometry.Set(Transform);
            foreach (var child in Children)
            {
                child.UpdateGeometry();
            }
        }
        /// <summary>
        /// Check if the geometry has changed
        /// </summary>
        /// <returns></returns>
        public bool CheckGeometry()
        {            
            HasChanged = false;
            if (!DynamicGeometry.Compare(Transform))
            {                
                HasChanged = true;
            }
            bool result = HasChanged;
            foreach (var child in Children)
            {
                result |= child.CheckGeometry();
            }
            return result;
        }
        /// <summary>
        /// Add a child bone in the skeletal hierarchy
        /// </summary>
        /// <param name="bone"></param>
        public void AddChildBone(Bone bone)
        {
            Children.Add(bone);
        }        
        /// <summary>
        /// Resets the change flag to indicate if anything has changed
        /// </summary>
        public void ResetChangeFlag()
        {
            HasChanged = false;
            foreach (var child in Children)
            {
                child.ResetChangeFlag();
            }
        }        
        /// <summary>
        /// Resets the geometry of the bone to the default
        /// </summary>
        public void ResetGeometry()
        {
            if(OriginalAvatarGeometry != null)
            {
                //ApplyGeometry(OriginalAvatarGeometry);
                MoveGeometry(OriginalAvatarGeometry);
                foreach (var child in Children)
                {
                    child.ResetGeometry();
                }
            }
        }            
        /// <summary>
        /// Initialize the bone geometry
        /// </summary>
        public void InitGeometry()
        {
            CurrentAvatarGeometry.Set(DynamicGeometry);
            PrevAvatarGeometry.Set(DynamicGeometry);
            OriginalAvatarGeometry.Set(DynamicGeometry);

            foreach (var child in Children)
            {
                child.InitGeometry();
            }
        }
        /// <summary>
        /// When switching between the Active Scene View window and Avatar Configuration it is necessary to re initialize some values
        /// so that we can check for changes on the bone without error
        /// </summary>
        public void ReInitGeometry()
        {
            CurrentAvatarGeometry.Set(DynamicGeometry);
            PrevAvatarGeometry.Set(DynamicGeometry);

            foreach (var child in Children)
            {
                child.ReInitGeometry();
            }
        }
        /// <summary>
        /// Steps the geometry, used for detecting when to store a change to history (undo/redo)
        /// </summary>
        public void StepGeometry()
        {
            PrevAvatarGeometry.Set(CurrentAvatarGeometry);
            CurrentAvatarGeometry.Set(DynamicGeometry);

            foreach (var child in Children)
            {
                child.StepGeometry();
            }
        }            
        /// <summary>
        /// Sets the default geometry
        /// </summary>
        public void SetOriginalGeometry()
        {
            OriginalAvatarGeometry.Set(CurrentAvatarGeometry);
            foreach (var child in Children)
            {
                child.SetOriginalGeometry();
            }
        }
        /// <summary>
        /// Apply a geometry to the bone
        /// </summary>
        /// <param name="geometry">AvatarTransform value to apply</param>
        public void ApplyGeometry(AvatarTransform geometry)
        {
            try
            {
                Transform.position = geometry.Position;
                Transform.rotation = geometry.Rotation;
                Transform.localPosition = geometry.LocalPosition;
                Transform.localRotation = geometry.LocalRotation;
                Transform.localScale = geometry.Scale;
                DynamicGeometry.Set(geometry);
                CurrentAvatarGeometry.Set(geometry);
                HasChanged = true;
            }
            catch(Exception e)
            {
                Debug.LogError(e.Message);
            }
        }
        /// <summary>
        /// Moves a GameObject transform to the target AvatarTransform geometry
        /// </summary>
        /// <param name="geometry">Target AvatarTransform to move the transform to</param>
        public void MoveGeometry(AvatarTransform geometry)
        {
            Transform.position = geometry.Position;
            Transform.rotation = geometry.Rotation;
            Transform.localPosition = geometry.LocalPosition;
            Transform.localRotation = geometry.LocalRotation;
            Transform.localScale = geometry.Scale;
            HasChanged = true;
        }
    }
}