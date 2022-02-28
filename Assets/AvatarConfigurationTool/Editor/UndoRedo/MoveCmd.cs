using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ACT.UndoRedo
{
    [Serializable]
    public class MoveCmd
    {            
        public string ModelName;
        public Bone bone;
        public List<MoveCmd> Children;
        public AvatarTransform CurrentGeometry;
        public AvatarTransform PrevGeometry;
        //public Geometry Geometry;

        /// <summary>
        /// Constructor
        /// </summary>
        public MoveCmd()
        {

        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="other">Bone to Move</param>
        public MoveCmd(Bone other)
        {
            bone = other;
            ModelName = other.ModelName;
            CurrentGeometry = new AvatarTransform(other.CurrentAvatarGeometry);
            PrevGeometry = new AvatarTransform(other.PrevAvatarGeometry);
            Children = new List<MoveCmd>();
            foreach (var child in other.Children)
            {
                MoveCmd childRecord = new MoveCmd(child);
                Children.Add(childRecord);
            }
        }
        /// <summary>
        /// Execute and Undo
        /// </summary>
        public void Undo()
        {            
            //bone.CurrentGeometry.Copy(PrevGeometry);
            if(bone.Transform == null)
            {
                Debug.Log("Null Transform: " + bone.ModelName);
            }
            bone.ApplyGeometry(PrevGeometry);
            foreach (var child in Children)
            {
                child.Undo();
            }
        }
        /// <summary>
        /// Execute a Redo
        /// </summary>
        public void Redo()
        {
            bone.ApplyGeometry(CurrentGeometry);
            //bone.CurrentAvatarGeometry.Copy(CurrentGeometry);
            foreach (var child in Children)
            {
                child.Redo();
            }
        }
    }
}