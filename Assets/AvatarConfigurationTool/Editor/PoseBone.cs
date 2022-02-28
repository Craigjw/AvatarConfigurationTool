using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace ACT
{
    [Serializable]
    public struct PoseBone
    {
        public string ModelName;
        public HumanBodyBones HumanName;
        public AvatarTransform Geometry;    
        
        /// <summary>
        /// Copy Constructor
        /// </summary>
        /// <param name="bone">Bone to copy</param>
        public PoseBone(Bone bone)
        {
            Geometry = new AvatarTransform(bone.DynamicGeometry);
            ModelName = bone.ModelName;
            HumanName = bone.HumanName;
        }
    }
}