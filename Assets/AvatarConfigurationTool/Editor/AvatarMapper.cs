using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ACT.UndoRedo;

namespace ACT
{
    public static class AvatarMapper
    {
        static int count = 0;
        //For reference only
        //private static HumanBodyBones[] fingerBoneIds = new HumanBodyBones[]
        //{
        //    HumanBodyBones.LeftIndexProximal,
        //    HumanBodyBones.LeftMiddleProximal,
        //    HumanBodyBones.LeftRingProximal,
        //    HumanBodyBones.LeftLittleProximal,
        //    HumanBodyBones.LeftThumbProximal,
        //    HumanBodyBones.RightIndexProximal,
        //    HumanBodyBones.RightMiddleProximal,
        //    HumanBodyBones.RightLittleProximal,
        //    HumanBodyBones.RightRingProximal,
        //    HumanBodyBones.RightThumbProximal,
        //};

        /// <summary>
        /// Configures a Skeleton data structure which contains all the meta data relating ACT
        /// </summary>
        /// <param name="source">GameObject source object</param>
        /// <returns>Skeleton data structure</returns>
        public static Skeleton ConfigureSkeleton(GameObject source)
        {
            Skeleton skeleton = new Skeleton();
            skeleton.FbxModelName = source.name;
            ConfigureBones(skeleton, source);
            ConfigureHipBone(skeleton, source);
            ConfigureHumanoidBones(skeleton, source);
            RemoveExtraneousTransforms(skeleton, source);
            ConfigureParentRelations(skeleton);            
            ConfigureSkeletalBones(skeleton);
            ConfigureHandBones(skeleton);
            NormalizeBones(skeleton);
            CheckTransforms(skeleton);
            //PrintBones(skeleton);
            return skeleton;
        }
        /// <summary>
        /// Reconfigures the Skeleton when we have switched from the regular Scene View window to an 
        /// Avatar Configuration Window as Transform references will be lost when switching.
        /// </summary>
        /// <param name="skeleton">Skeleton to be reconfigured</param>
        /// <param name="source">Source GameObject in the active scene view window</param>
        public static void ReconfigureSkeleton(Skeleton skeleton, GameObject source)
        {
            ApplyTransformReferences(skeleton, source);
            ReconfigureBones(skeleton, source);
        }
        /// <summary>
        /// Loads Skeleton data from a file into a Skeleton data structure
        /// </summary>
        /// <param name="skeleton">Existing Skeleton data to populate</param>
        /// <param name="source">Source GameObject in the active scene view window</param>
        public static void LoadSkeleton(Skeleton skeleton, GameObject source)
        {            
            ConfigureHipBone(skeleton, source);
            ApplyTransformReferences(skeleton, source);
            ApplyHistoryBoneReferences(skeleton);
            ConfigureParentRelations(skeleton);
            ReconfigureBones(skeleton, source);
            CheckTransforms(skeleton);
            ReverseUndoHistoryStack(skeleton);
            StitchCurrentPoseToUndoStack(skeleton);
        }
        /// <summary>
        /// Loads Inactive skeleton.  When the Avatar Configuration Window open, this will load the 
        /// skeleton for the Scene View Window which is no longer active and vice versa.  Loaded Skeleton will need to be Reconfigured
        /// when we switch to a different scene.
        /// </summary>
        /// <param name="skeleton">Skeleton to load</param>
        public static void LoadInactiveSkeleton(Skeleton skeleton)
        {
            ConfigureHipBone(skeleton);
            ApplyHistoryBoneReferences(skeleton);
            ConfigureInactiveParentRelations(skeleton);
            //CheckTransforms(skeleton);
            ReverseUndoHistoryStack(skeleton);
            //ReApplyUndoBoneReferences(skeleton);
            //StitchCurrentPoseToUndoStack(skeleton);
            //PrintBones(skeleton);
            //PrintChildBones(skeleton.HipBone, "");
        }
        
        /// <summary>
        /// Checks skeleton transforms are valid
        /// </summary>
        /// <param name="skeleton"></param>
        private static void CheckTransforms(Skeleton skeleton)
        {
            RecurseChild(skeleton.HipBone);

            void RecurseChild(Bone bone)
            {
                if(bone.Transform == null)
                {
                    Debug.LogError("Bone Transform Null: " + bone.ModelName);
                }
                foreach (var child in bone.Children)
                    RecurseChild(child);
            }
        }
        ///////////////////////////////////////
        //For debug purposes only//
        ///////////////////////////////////////
        private static void PrintChildBones(Bone bone, string prefix)
        {
            if (bone == null)
                Debug.LogError("Null Bone");
            else
            {
                Debug.Log("[" + count + "] " + prefix + ": " + bone.ModelName + " - " + bone.HumanName);

                count++;
                foreach (var child in bone.Children)
                {
                    PrintChildBones(child, prefix + "-");
                }
            }
        }
        private static void PrintBones(Skeleton skeleton)
        {
            foreach (var bone in skeleton.Bones.Values)
            {
                if (bone == null)
                    return;
                if (bone == null)
                    Debug.LogError("Null Bone");
                else
                    Debug.Log(bone.ModelName + " - " + bone.HumanName);
            }
            //PrintBonesRecursion(skeleton.HipBone, "");            
        }
        private static void PrintBonesRecursion(Bone bone, string prefix)
        {
            Debug.Log(prefix + " " + bone.ModelName);
            foreach (var child in bone.Children)
            {
                PrintBonesRecursion(child, prefix + "-");
            }
        }
        ///////////////////////////////////////
        //End of Debug code//
        ///////////////////////////////////////
  
        /// <summary>
        /// Configures Skeletal Bones from Source GameObject
        /// </summary>
        /// <param name="skeleton">Skeleton to configure</param>
        /// <param name="source">Source GameObject</param>
        private static void ConfigureBones(Skeleton skeleton, GameObject source)
        {
            var boneTransforms = GetBoneTransforms(source);
            foreach (var transform in boneTransforms)
            {
                var bone = ConfigureBone(transform);
                skeleton.AddBone(bone);
            }
        }
        /// <summary>
        /// Gets the relevant transforms for a GameObject
        /// </summary>
        /// <param name="source">Source GameObject</param>
        /// <returns>List of Transforms found from the source</returns>
        private static List<Transform> GetBoneTransforms(GameObject source)
        {
            var transforms = new List<Transform>();
            foreach (var smRenderers in source.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                foreach (var smr in smRenderers.bones)
                {
                    // Don't add duplicates
                    if (transforms.Contains(smr))
                    {
                        continue;
                    }
                    transforms.Add(smr);
                }
            }

            var humanBoneTransforms = GetHumanoidTransforms(source);
            foreach (var bone in humanBoneTransforms)
            {
                if (!transforms.Contains(bone))
                    transforms.Add(bone);
            }
            var humanTransforms = GetHumanoidTransformsLookup(source);
            return transforms;
        }        
        /// <summary>
        /// Configures a new Skeletal Bone from a transform
        /// </summary>
        /// <param name="transform">Source transform to use for the configuration</param>
        /// <returns>Configured Bone</returns>
        private static Bone ConfigureBone(Transform transform)
        {
            Bone bone = new Bone();
            bone.ModelName = transform.name;
            bone.HumanName = HumanBodyBones.LastBone;
            bone.Transform = transform;
            bone.Parent = transform.parent;
            var geometry = new AvatarTransform(transform.position, transform.localPosition, transform.rotation, transform.localRotation, transform.localScale);
            bone.DynamicGeometry = geometry;
            bone.IsHumanBone = false;
            bone.IsSkeletal = false;
            bone.IsRoot = false;
            bone.InitGeometry();
            return bone;
        }
        /// <summary>
        /// Configures the HipBone within the Skeleton
        /// </summary>
        /// <param name="skeleton">Skeleton which will have it's HipBone configured</param>
        /// <param name="source">Source GameObject which contains the hipbone</param>
        private static void ConfigureHipBone(Skeleton skeleton, GameObject source = null)
        {
            Bone hipBone = null;
            if (source == null)
            {
                hipBone = skeleton.GetBone(HumanBodyBones.Hips);
                if (hipBone != null)
                    skeleton.HipBone = hipBone;
            }
            else
            {
                var hipTransform = GetHipTransform(source);
                hipBone = skeleton.GetBone(hipTransform.name);
                if (hipBone != null)
                    skeleton.HipBone = hipBone;
            }
        }
        /// <summary>
        /// Returns the HipBone Transform for the GameObject source
        /// </summary>
        /// <param name="source">GameObject source character</param>
        /// <returns>HipBone Transform</returns>
        private static Transform GetHipTransform(GameObject source)
        {
            Transform hipBone = null;
            var animators = source.GetComponentsInChildren<Animator>();
            foreach (var animator in animators)
            {
                if (!animator.isHuman)
                    continue;
                foreach (var boneId in System.Enum.GetValues(typeof(HumanBodyBones)))
                {
                    if ((HumanBodyBones)boneId == HumanBodyBones.Hips)
                    {
                        hipBone = animator.GetBoneTransform((HumanBodyBones)boneId);
                        break;
                    }
                }
            }
            return hipBone;
        }
        /// <summary>
        /// Configures Humanoid Bones for the given GameObject source
        /// </summary>
        /// <param name="skeleton">Skeleton to configure</param>
        /// <param name="source">Source GameObject</param>
        private static void ConfigureHumanoidBones(Skeleton skeleton, GameObject source)
        {
            skeleton.HumanBonesLookup.Clear();
            var humanoidTransforms = GetHumanoidTransformsLookup(source);
            foreach(var kvp in humanoidTransforms)
            {
                HumanBodyBones boneId = kvp.Key;
                Transform transform = kvp.Value;
                Bone bone = null;

                skeleton.Bones.TryGetValue(transform.name, out bone);
                if (bone == null)
                    Debug.Log("Can't find transform." + transform.name);
                if(bone != null)
                {
                    bone.HumanName = boneId;
                    bone.IsHumanBone = true;
                    skeleton.HumanBonesLookup.Add(boneId, transform.name);
                }
            }
        }
        /// <summary>
        /// Gets the Humanoid Transforms for a given GameObject
        /// </summary>
        /// <param name="source">GameObject Source</param>
        /// <returns>List of Humanoid Transforms</returns>
        private static List<Transform> GetHumanoidTransforms(GameObject source)
        {
            var bones = new List<Transform>();
            var animators = source.GetComponentsInChildren<Animator>();
            foreach (var animator in animators)
            {
                if (!animator.isHuman)
                    continue;
                foreach (var boneId in System.Enum.GetValues(typeof(HumanBodyBones)))
                {
                    HumanBodyBones boneKey = (HumanBodyBones)boneId;
                    if (boneKey == HumanBodyBones.LastBone)
                        break;
                    var boneTransform = animator.GetBoneTransform(boneKey);
                    if (boneTransform == null || bones.Contains(boneTransform))
                        continue;
                    bones.Add(boneTransform);
                }
            }
            return bones;
        }
        /// <summary>
        /// Gets a Humanoid Transform Lookup dictionary relating HumanBodyBones enumeration to their corresponding Transforms
        /// </summary>
        /// <param name="source">Source GameObject</param>
        /// <returns>Dictionary containing HumanBodyBones to Transforms Lookup</returns>
        private static Dictionary<HumanBodyBones, Transform> GetHumanoidTransformsLookup(GameObject source)
        {
            var bones = new Dictionary<HumanBodyBones, Transform>();
            var animators = source.GetComponentsInChildren<Animator>();
            foreach (var animator in animators)
            {
                if (!animator.isHuman)
                    continue;
                foreach (var boneId in System.Enum.GetValues(typeof(HumanBodyBones)))
                {
                    HumanBodyBones boneKey = (HumanBodyBones)boneId;
                    if (boneKey == HumanBodyBones.LastBone)
                        break;
                    var boneTransform = animator.GetBoneTransform(boneKey);
                    if (boneTransform == null || bones.ContainsKey(boneKey))
                        continue;
                    bones.Add(boneKey, boneTransform);
                }
            }
            return bones;
        }
        /// <summary>
        /// Remove extraneous bones which are not part of the Humanoid bone skeleton
        /// </summary>
        /// <param name="skeleton">Skeleton to remove bones from</param>
        /// <param name="source">Source GameObject</param>
        private static void RemoveExtraneousTransforms(Skeleton skeleton, GameObject source)
        {
            var extraneousRootTransforms = GetExtraneousRootTransforms(skeleton, source);
            foreach (var transform in extraneousRootTransforms)
            {
                skeleton.Bones.Remove(transform.name);
            }
        }
        /// <summary>
        /// Gets extraneous transforms leading upto the root transform of the Humanoid Bones.
        /// </summary>
        /// <param name="skeleton">Skeleton to configure</param>
        /// <param name="source">Sourec GameObject</param>
        /// <returns>List of Transforms to be removed from the skeleton</returns>
        private static List<Transform> GetExtraneousRootTransforms(Skeleton skeleton, GameObject source)
        {
            List<Transform> rootBones = new List<Transform>();
            rootBones.AddRange(RecurseRootTransforms(skeleton.HipBone.Transform));
            rootBones.Remove(skeleton.HipBone.Transform);
            return rootBones;
        }
        /// <summary>
        /// Recurses through the Root transforms
        /// </summary>
        /// <param name="transform">Transform to recurse</param>
        /// <returns>Additive list of Recursed Transforms</returns>
        private static List<Transform> RecurseRootTransforms(Transform transform)
        {
            List<Transform> childTransforms = new List<Transform>();
            if (transform == null)
                return childTransforms;
            var parent = transform.parent;
            List<Transform> children = new List<Transform>();
            if (parent != null)
            {
                for (int i = 0; i < parent.childCount; i++)
                {
                    var child = parent.GetChild(i);
                    children.Add(child);
                }
            }
            children.AddRange(RecurseRootTransforms(parent));
            return children;
        }
        /// <summary>
        /// Configure parental Relations within the skeleton
        /// </summary>
        /// <param name="skeleton">Skeleton to configure</param>
        private static void ConfigureParentRelations(Skeleton skeleton)
        {
            foreach (var bone in skeleton.Bones.Values)
            {
                if (bone == null)
                    continue;
                if (bone.HumanName != HumanBodyBones.Hips)
                {
                    var parentTransform = bone.Transform.parent;
                    if (parentTransform != null)
                    {
                        var parentBone = skeleton.GetBone(parentTransform.name);
                        if (parentBone != null)
                        {
                            bone.Parent = parentTransform;
                            bone.ParentBone = parentBone;
                            bone.ParentBoneModelName = parentBone.ModelName;
                            parentBone.AddChildBone(bone);
                        }
                    }
                    else
                    {
                        Debug.Log("No Parent Transform");
                    }
                }
            }
        }
        /// <summary>
        /// Configure parental relations for an Inactive skeleton, which will have missing transform references due
        /// to the scene not being currently active.
        /// </summary>
        /// <param name="skeleton">Skeleton to configure</param>
        private static void ConfigureInactiveParentRelations(Skeleton skeleton)
        {
            foreach (var bone in skeleton.Bones.Values)
            {
                if (bone == null)
                    continue;
                if (bone.HumanName != HumanBodyBones.Hips)
                {
                    if (bone.ParentBoneModelName != null && bone.ParentBoneModelName != string.Empty)
                    {
                        var parentBone = skeleton.GetBone(bone.ParentBoneModelName);
                        if(parentBone != null)
                        {
                            bone.ParentBone = parentBone;
                            parentBone.AddChildBone(bone);
                        }
                    }                                        
                    else
                    {
                        Debug.LogError("No Parent found!");
                    }
                }
            }
        }
        /// <summary>
        /// Configure Skeletal bones, some bones that are not marked as Humanoid may be located between other Humanoid bones
        /// We mark these as skeletal so that we don't break the skeleton.
        /// </summary>
        /// <param name="skeleton">Skeleton to configure</param>
        private static void ConfigureSkeletalBones(Skeleton skeleton)
        {
            foreach (var bone in skeleton.Bones.Values)
            {
                if (bone == null)
                    continue;
                if(bone.HumanName != HumanBodyBones.Hips &&
                    bone.HumanName != HumanBodyBones.LastBone)
                    RecurseConfigureSkeletalBones(bone);
            }
            void RecurseConfigureSkeletalBones(Bone bone)
            {
                bone.IsSkeletal = true;
                if (bone.HumanName != HumanBodyBones.Hips)
                    RecurseConfigureSkeletalBones(bone.ParentBone);
            }
        }        
        /// <summary>
        /// Configure Hand bones so that separate Gizmos can be drawn for them
        /// </summary>
        /// <param name="skeleton">Skeleton to configure</param>
        public static void ConfigureHandBones(Skeleton skeleton)
        {
            if (skeleton.HipBone != null)
                ConfigureHandBoneRecurse(skeleton.HipBone, false);
            
            ///Recurse through bones, once we reach a hand bone, all further bones can be considered to be hand bones
            void ConfigureHandBoneRecurse(Bone bone, bool isHandBone)
            {
                if (bone != null)
                {
                    if (isHandBone == true)
                    {
                        bone.IsHandBone = true;
                        foreach (var child in bone.Children)
                        {
                            ConfigureHandBoneRecurse(child, true);
                        }
                    }
                    else
                    {
                        if (bone.HumanName == HumanBodyBones.LeftHand || bone.HumanName == HumanBodyBones.RightHand)
                        {
                            bone.IsHandBone = true;
                            foreach (var child in bone.Children)
                            {
                                ConfigureHandBoneRecurse(child, true);
                            }
                        }
                        else
                        {
                            foreach (var child in bone.Children)
                                ConfigureHandBoneRecurse(child, false);
                        }
                    }
                }
            }
        }        
        /// <summary>
        /// Normalize the Bones Dictionary, so that the Root bones appear first, purely cosmetic.
        /// </summary>
        /// <param name="skeleton">Skeleton to normalize for</param>
        private static void NormalizeBones(Skeleton skeleton)
        {
            Dictionary<string, Bone> NormalizedBones = new Dictionary<string, Bone>();
            NormalizeBonesRecursion(NormalizedBones, skeleton.HipBone);
            skeleton.Bones = NormalizedBones;
        }
        /// <summary>
        /// Recurse each Bone in the Dictionary for normalization
        /// </summary>
        /// <param name="bones">Bones dictionary to normalize</param>
        /// <param name="bone">Current bone we have recursed to</param>
        private static void NormalizeBonesRecursion(Dictionary<string, Bone> bones, Bone bone)
        {
            bones.Add(bone.ModelName, bone);
            foreach (var child in bone.Children)
            {
                NormalizeBonesRecursion(bones, child);
            }
        }
        /// <summary>
        /// When we load a skeleton, the undo stack will not refer to the current pose, we stitch the current pose to the most recent
        /// undo stack command, if there is one.
        /// </summary>
        /// <param name="skeleton">Skeleton which the undo stack is to be stitched</param>
        public static void StitchCurrentPoseToUndoStack(Skeleton skeleton)
        {
            if (skeleton.History.undo.Count > 0)
            {
                //When loading, the current pose is added to the undo stack and stitched to the loaded stack
                skeleton.DoCmd();
                var item1 = skeleton.History.undo.Pop();
                var item2 = skeleton.History.undo.Pop();

                RecurseChildren(item1, item2);
                skeleton.History.undo.Push(item2);
                skeleton.History.undo.Push(item1);

                void RecurseChildren(MoveCmd cmd1, MoveCmd cmd2)
                {
                    cmd1.PrevGeometry = cmd2.CurrentGeometry;
                    for (int i = 0; i < cmd1.Children.Count; i++)
                    {
                        RecurseChildren(cmd1.Children[i], cmd2.Children[i]);
                    }
                }
            }
        }
        /// <summary>
        /// This is not used, maybe needed later.
        /// </summary>
        /// <param name="skeleton"></param>
        private static void ReApplyUndoBoneReferences(Skeleton skeleton)
        {
            //foreach(var cmd in skeleton.History.undo)
            //{
            //    if (cmd.bone != null)
            //    {
            //        var bone = skeleton.GetBone(cmd.bone.ModelName);
            //        cmd.bone = bone;
            //    }
            //    else
            //        Debug.LogError("UndoCmd Bone is null");                
            //}
        }
        /// <summary>
        /// When de-serializing the undo stack from file, it becomes reversed
        /// </summary>
        /// <param name="skeleton">Skeleton which contains the undo stack</param>
        private static void ReverseUndoHistoryStack(Skeleton skeleton)
        {
            //Undo stack gets de-serialized in reverse
            var newUndoStack = new Stack<MoveCmd>();
            int undoCount = skeleton.History.undo.Count;
            for (int i = 0; i < undoCount; i++)
            {
                var item = skeleton.History.undo.Pop();
                newUndoStack.Push(item);
            }
            skeleton.History.undo = newUndoStack;
        }     
        /// <summary>
        /// Recursively reconfiguring the Bones is required when switching from between the Scene View Window and the Avatar Configuration Window
        /// </summary>
        /// <param name="skeleton">Skeleton to reconfigure</param>
        /// <param name="source">Source GameObject</param>
        public static void ReconfigureBones(Skeleton skeleton, GameObject source)
        {
            foreach(var bone in skeleton.Bones.Values)
            {
                ReconfigureBone(bone);
            }
        }
        /// <summary>
        /// Reconfigure a bone, this applies transform references to the Bone data structure and sets up the geometry
        /// </summary>
        /// <param name="bone"></param>
        public static void ReconfigureBone(Bone bone)
        {
            var geometry = new AvatarTransform(bone.Transform.position, bone.Transform.localPosition, bone.Transform.rotation, bone.Transform.localRotation, bone.Transform.localScale);
            bone.DynamicGeometry = geometry;
            bone.ReInitGeometry();
        }
        /// <summary>
        /// Apply transform references to the skeletal bones from a source GameObject
        /// </summary>
        /// <param name="skeleton">Skeleton to apply transform references to</param>
        /// <param name="source">Source GameObject</param>
        public static void ApplyTransformReferences(Skeleton skeleton, GameObject source)
        {
            var transforms = GetBoneTransforms(source);
            foreach (var transform in transforms)
            {
                var bone = skeleton.GetBone(transform.name);
                if (bone != null)
                    bone.Transform = transform;
            }
        }
        /// <summary>
        /// Apply Transform references to History used in Undo/Redo
        /// </summary>
        /// <param name="skeleton">Skeleton to apply historical transform references to</param>
        public static void ApplyHistoryBoneReferences(Skeleton skeleton)
        {
            ApplyUndoBoneReferences(skeleton);
            ApplyRedoBoneReferences(skeleton);
        }
        /// <summary>
        /// Apply Transform references to the Undo history
        /// </summary>
        /// <param name="skeleton">Skeleton to apply references to</param>
        private static void ApplyUndoBoneReferences(Skeleton skeleton)
        {
            foreach (var undo in skeleton.History.undo)
            {
                RecurseUndo(undo);
            }
            void RecurseUndo(MoveCmd undo)
            {
                Bone bone = null;
                if (skeleton.Bones.TryGetValue(undo.ModelName, out bone))
                {
                    undo.bone = bone;
                }
                foreach (var child in undo.Children)
                    RecurseUndo(child);
            }
        }
        /// <summary>
        /// Apply Transform references to the Redo history
        /// </summary>
        /// <param name="skeleton">Skeleton to apply references to</param>
        private static void ApplyRedoBoneReferences(Skeleton skeleton)
        {
            foreach (var redo in skeleton.History.redo)
            {
                RecurseUndo(redo);
            }
            void RecurseUndo(MoveCmd redo)
            {
                Bone bone = null;
                if (skeleton.Bones.TryGetValue(redo.ModelName, out bone))
                {
                    redo.bone = bone;
                }
                foreach (var child in redo.Children)
                    RecurseUndo(child);
            }
        }
    }
}