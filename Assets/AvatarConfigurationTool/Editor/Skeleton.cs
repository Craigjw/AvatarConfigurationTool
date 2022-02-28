using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using ACT.UndoRedo;

namespace ACT
{    
    [Serializable]
    public class Skeleton
    {
        public string FbxModelName;

        [NonSerialized] public Bone RootBone;
        [NonSerialized] public Bone HipBone;

        public History History;

        public Dictionary<string, Bone> Bones;
        public Dictionary<HumanBodyBones, string> HumanBonesLookup;

        /// <summary>
        /// Constructor
        /// </summary>
        public Skeleton()
        {
            Bones = new Dictionary<string, Bone>();
            HumanBonesLookup = new Dictionary<HumanBodyBones, string>();
            History = new History();
        }        
        /// <summary>
        /// Updates the geometry when the transforms are moved
        /// </summary>
        public void UpdateGeometry()
        {
            if(HipBone != null)
            {
                HipBone.UpdateGeometry();
            }
        }
        /// <summary>
        /// Checks Geometry for moved transforms
        /// </summary>
        /// <returns></returns>
        public bool CheckGeometry()
        {
            if(HipBone != null)
            {
                return HipBone.CheckGeometry();
            }
            return false;
        }
        /// <summary>
        /// Resets the change flag
        /// </summary>
        public void ResetChangeFlag()
        {
            if (HipBone != null)
                HipBone.ResetChangeFlag();
        }
        /// <summary>
        /// Execute a Do command, we have moved something on the character, record this in history
        /// </summary>
        public void DoCmd()
        {
            var record = new MoveCmd(HipBone);
            History.Do(record);
        }
        /// <summary>
        /// Execute an Undo command, record in history
        /// </summary>
        public void UndoCmd()
        {
            History.Undo();
        }
        /// <summary>
        /// Execute a Redo command, record this in history
        /// </summary>
        public void Redo()
        {
            History.Redo();
        }
        /// <summary>
        /// Validate that the Skeleton is correct
        /// </summary>
        /// <returns></returns>
        public bool Validate()
        {
            if (FbxModelName != string.Empty                
                && RootBone != null
                && HipBone != null)
                return true;
            return false;
        }
        /// <summary>
        /// Apply a pose to the skeleton
        /// </summary>
        /// <param name="pose">Pose to apply</param>
        public void ApplyPose(Pose pose)
        {
            foreach(var poseBone in pose.Bones)
            {
                var bone = GetBone(poseBone.ModelName);
                if(bone != null)
                {
                    bone.ApplyPoseBone(poseBone);
                }
            }
        }
        /// <summary>
        /// Apply the original pose geometry to the skeleton
        /// </summary>
        /// <param name="bones">List of Bones in the apply</param>
        public void ApplyOriginalGeometry(Dictionary<string, Bone> bones)
        {
            foreach(var otherBone in bones.Values)
            {
                var bone = GetBone(otherBone.ModelName);
                bone.OriginalAvatarGeometry = otherBone.OriginalAvatarGeometry;
            }            
        }
        /// <summary>
        /// Sets the default geometry of the skeleton
        /// </summary>
        public void SetOriginalGeometry()
        {
            foreach (var bone in Bones.Values)
            {                
                bone.SetOriginalGeometry();
            }
        }
        /// <summary>
        /// Gets a bone given a HumanBodyBones enumeration value
        /// </summary>
        /// <param name="boneId">Enumerated value to search</param>
        /// <returns>Bone that is found from the search</returns>
        public Bone GetBone(HumanBodyBones boneId)
        {
            if (HumanBonesLookup.TryGetValue(boneId, out string modelName))
            {
                if (modelName != string.Empty)
                {
                    if(Bones.TryGetValue(modelName, out Bone result))
                        return result;
                }
            }
            return null;
        }
        /// <summary>
        /// Gets a bone given a string model name
        /// </summary>
        /// <param name="modelName">model name to search for</param>
        /// <returns>Bone that is found from the search</returns>
        public Bone GetBone(string modelName)
        {
            List<Bone> results = new List<Bone>();
            foreach (var bone in Bones)
            {
                if (bone.Value.ModelName == modelName)
                    results.Add(bone.Value);
            }
            if (results.Count > 0)
                return results.First();
            else
                return null;
        }
        /// <summary>
        /// Add a bone to the skeleton
        /// </summary>
        /// <param name="bone">bone to add</param>
        public void AddBone(Bone bone)
        {
            if (!Bones.ContainsKey(bone.ModelName))
                Bones.Add(bone.ModelName, bone);      
            if(bone.HumanName != HumanBodyBones.LastBone)
                HumanBonesLookup.Add(bone.HumanName, bone.ModelName);            
        }
        /// <summary>
        /// Gets an array of the bone Parents
        /// </summary>
        /// <param name="bone">Bone to get parents for</param>
        /// <returns>Array of Bone parents</returns>
        public Bone[] GetParents(Bone bone)
        {
            List<Bone> results = new List<Bone>();
            AddParentsRecursion(results, bone);
            return results.ToArray();
        }
        /// <summary>
        /// Recursively add parental bones to the current bone
        /// </summary>
        /// <param name="bones"></param>
        /// <param name="bone"></param>
        private void AddParentsRecursion(List<Bone> bones, Bone bone)
        {
            if (bone.HumanName != HumanBodyBones.LastBone && bone.HumanName == HumanBodyBones.Hips)
                return;
            else            
            {
                bones.Add(bone);
                AddParentsRecursion(bones, bone.ParentBone);
            }   
        }
        /// <summary>
        /// Steps the geometry, used to detect changes so that we may record it's history
        /// </summary>
        public void StepGeometry()
        {
            HipBone.StepGeometry();
        }        
        /// <summary>
        /// Reset the geometry of the Skeleton to default
        /// </summary>
        public void ResetGeometry()
        {
            HipBone.ResetGeometry();           
        }
        /// <summary>
        /// Gets an array of all GameObjects from the List of skeletal Bones
        /// </summary>
        /// <returns>Array of Bone GameObjects</returns>
        public GameObject[] GetAllObjects()
        {
            var gos = new List<GameObject>();
            foreach(var kvp in Bones)
            {
                gos.Add(kvp.Value.Transform.gameObject);
            }
            return gos.ToArray();
        }
    }
}
