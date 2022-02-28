using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ACT
{
    [Serializable]
    public class Pose
    {
        public string FbxModelName;
        public List<PoseBone> Bones;

        /// <summary>
        /// Constructor
        /// </summary>
        public Pose()
        {

        }
        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="skeleton"></param>
        public Pose(Skeleton skeleton)
        {
            FbxModelName = skeleton.FbxModelName;
            CopySkeleton(skeleton);
        }
        /// <summary>
        /// Copy Skeleton
        /// </summary>
        /// <param name="skeleton">Skeleton to copy</param>
        private void CopySkeleton(Skeleton skeleton)
        {
            Bones = new List<PoseBone>();
            foreach(var bone in skeleton.Bones.Values)
            {
                PoseBone poseBone = new PoseBone(bone);
                Bones.Add(poseBone);
            }
        }        
    }
}