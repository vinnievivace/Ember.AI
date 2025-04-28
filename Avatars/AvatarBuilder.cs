using System;
using System.Collections.Generic;
using EmberAI.Core;
using EmberAI.Core.Util;
using UnityEditor;
using UnityEngine;

namespace EmberAI.Avatars
{
    /// <summary>
    /// Util to prepare AlteredState generated avatar GLB for Humanoid rigging / Animation systems
    /// </summary>
    public static class AvatarBuilder
    {
        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        [Obsolete("unused", true)]
        public static void Initialize(Transform target, AvatarConfig config)
        {
            if(!config.applyRotationFix) return;
            
            // right now the exported GLB has root rotated, so we need to fix that
            Transform root = target.Find(config.rootName);
            
            if(root == null) throw new Exception("Root transform not found in target hierarchy.");
            
            Vector3 rootPosition = root.position;
            
            Transform rootX = root.Find("root.x");
            
            // TODO implement extensions
            root.ResetLocalTransform();
            rootX.ResetLocalTransform();
            
            Debug.LogWarning("need to implement extensions");
            
            root.position = rootPosition;

        }

        public static bool IsValid(Transform target, AvatarConfig config)
        {
            if (target == null)
            {
                Debug.LogError("Target Transform is null.");
                return false;
            }

            if (config == null || config.bones == null || config.bones.Count == 0)
            {
                Debug.LogError("AvatarConfig or its bones list is null or empty.");
                return false;
            }

            foreach (var boneConfig in config.bones)
            {
                if (string.IsNullOrEmpty(boneConfig.target))
                {
                    Debug.LogWarning($"Bone {boneConfig.BoneID} has an empty target name.");
                    return false;
                }

                Transform boneTransform = target.FindChildTransform(boneConfig.target);
                
                if (boneTransform == null)
                {
                    Debug.LogError($"Target Transform is missing required child: {boneConfig.target} for bone {boneConfig.BoneID}.");
                    return false;
                }
            }

            DebugConfig(target, config);
            
            return true;
        }

        public static void Build(Transform target, AvatarConfig config, string assetPath)
        {
            if (!IsValid(target, config))
            {
                Debug.LogError("Cannot build Avatar. The target or config is invalid.");
                return;
            }
            
            // when building at runtime, re apply the existing avatar
            
            // store position and rotation to allow reset once Avatar is build
            // this is important to ensure Avatar builds correctly
            Vector3 originalPosition = target.position;
            Quaternion originalRotation = target.rotation;
            
            target.ResetLocalTransform();
            
            HumanDescription humanDescription = new HumanDescription();
            Transform root = (config.rootName == "") ? target : target.FindChildTransform(config.rootName);
            
            if(root == null) throw new Exception("Root transform not found in target hierarchy. looking for " + config.rootName);
            
            var humanBones = new HumanBone[config.bones.Count];
            int index = 0;
            
            foreach (AvatarBoneConfig boneConfig in config.bones)
            {
                Transform boneTransform = root.FindChildTransform(boneConfig.target);

                if (boneTransform == null)
                {
                    Debug.LogError($"Bone '{boneConfig.BoneID}' (target transform: '{boneConfig.target}') not found in target hierarchy. Root = " + root.name);

                    return;
                }

                humanBones[index] = new HumanBone
                {
                    boneName = boneConfig.target,
                    humanName = boneConfig.BoneID.ToString(),
                    limit = new HumanLimit { useDefaultValues = true }
                };

                
                
                index++;
            }
            
            var skeletonBones = GetSkeletonBones(root, config);
            
            //ApplyBoneFixes(skeletonBones, config);
            
            humanDescription.human = humanBones;
            humanDescription.skeleton = skeletonBones;
            humanDescription.armStretch = 0.05f;
            humanDescription.legStretch = 0.05f;
            humanDescription.upperArmTwist = 0.5f;
            humanDescription.lowerArmTwist = 0.5f;
            humanDescription.upperLegTwist = 0.5f;
            humanDescription.lowerLegTwist = 0.5f;
            humanDescription.feetSpacing = 0f;
            humanDescription.hasTranslationDoF = false;

            Avatar avatar = null;
           
            try
            {
                avatar = SaveAvatar(root.gameObject, humanDescription, assetPath);
            }
            catch (Exception e)
            {
                throw new Exception($"Avatar creation failed. Please check bone mappings and hierarchy.", e);
            }

            Animator animator = target.GetOrAddComponent<Animator>();
            animator.avatar = avatar;
            animator.runtimeAnimatorController = config.animatorController;
            
            target.position = originalPosition;
            target.rotation = originalRotation;

            Debug.Log($"Avatar successfully created and set for the target at '{assetPath}'.");
        }

        private static Avatar SaveAvatar(GameObject target, HumanDescription humanDescription, string assetPath)
        {
            Avatar avatar = null;
           
            try
            {
                avatar = UnityEngine.AvatarBuilder.BuildHumanAvatar(target, humanDescription);
            }
            catch (Exception e)
            {
                throw new Exception($"Avatar creation failed. Please check bone mappings and hierarchy.", e);
            }

            if (!avatar.isValid || !avatar.isHuman)
            {
                Debug.LogError("Avatar creation failed. Please check bone mappings and hierarchy.");
                return null;
            }

            AssetDatabase.CreateAsset(avatar, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            return avatar;
        }

        public static void DebugConfig(Transform target, AvatarConfig config)
        {
            if (target == null)
            {
                UnityEngine.Debug.LogError("Target Transform is null. Cannot debug avatar.");
                return;
            }

            if (config == null || config.bones == null || config.bones.Count == 0)
            {
                UnityEngine.Debug.LogError("AvatarConfig or its bones list is null or empty. Cannot debug avatar.");
                return;
            }

            foreach (var boneConfig in config.bones)
            {
                // Find the target transform for the current bone
                Transform boneTransform = target.FindChildTransform(boneConfig.target);
                if (boneTransform == null)
                {
                    UnityEngine.Debug.LogWarning(
                        $"Couldn't find transform '{boneConfig.target}' for bone '{boneConfig.BoneID}'.");
                    continue;
                }

                // Draw a sphere-like debug point at the bone's position using Debug.DrawLine (simulating spheres)
                Debug.DrawLine(boneTransform.position, boneTransform.position + Vector3.up * 0.01f, Color.green, 0.1f);

                // Draw a line to visualize the hierarchy if the bone has a parent
                if (boneTransform.parent != null)
                {
                    Debug.DrawLine(boneTransform.parent.position, boneTransform.position, Color.yellow, 0.1f);
                }
            }
        }

        public static void CompareAvatars(GameObject target, Avatar avatar1, Avatar avatar2)
        {
            // Validate that both avatars are not null
            if (avatar1 == null || avatar2 == null)
            {
                Debug.LogError("One or both avatars are null. Cannot compare.");
                return;
            }

            // Validate that both avatars are human
            if (!avatar1.isHuman || !avatar2.isHuman)
            {
                Debug.LogError("One or both avatars are not human. Cannot compare.");
                return;
            }

            // Validate that both avatars are valid
            if (!avatar1.isValid || !avatar2.isValid)
            {
                Debug.LogError("One or both avatars are not valid. Cannot compare.");
                return;
            }

            HumanDescription humanDescription1 = avatar1.humanDescription;
            HumanDescription humanDescription2 = avatar2.humanDescription;

            // Compare human bones
            Debug.Log("Comparing human bones...");
            if (humanDescription1.human != null && humanDescription2.human != null)
            {
                if (humanDescription1.human.Length != humanDescription2.human.Length)
                {
                    Debug.LogWarning($"Mismatch in number of human bones: {humanDescription1.human.Length} vs {humanDescription2.human.Length}");
                    
                }

                for (int i = 0; i < Math.Max(humanDescription1.human.Length, humanDescription2.human.Length); i++)
                {
                    HumanBone? bone1 = (i < humanDescription1.human.Length) ? humanDescription1.human[i] : (HumanBone?)null;
                    HumanBone? bone2 = (i < humanDescription2.human.Length) ? humanDescription2.human[i] : (HumanBone?)null;

                    if (bone1 == null && bone2 != null)
                    {
                        Debug.LogWarning("Avatar 1 missing bone: " + bone2.Value.humanName);
                    }
                    else if (bone2 == null && bone1 != null)
                    {
                        Debug.LogWarning("Avatar 2 missing bone: " + bone1.Value.humanName);
                    }
                    else if (bone1 != null)
                    {
                        if (bone1.Value.humanName != bone2.Value.humanName || bone1.Value.boneName != bone2.Value.boneName)
                        {
                            Debug.LogWarning($"Bone mismatch at index {i}: " + $"- Avatar 1 Bone: {bone1.Value.boneName} ({bone1.Value.humanName}) " + $"- Avatar 2 Bone: {bone2.Value.boneName} ({bone2.Value.humanName})");
                        }
                    }
                }
            }
            else
            {
                Debug.LogWarning("One or both avatars have no human bones.");
            }

            // Compare skeleton bones
            Debug.Log("Comparing skeleton bones...");
            if (humanDescription1.skeleton != null && humanDescription2.skeleton != null)
            {
                if (humanDescription1.skeleton.Length != humanDescription2.skeleton.Length)
                {
                    Debug.LogWarning($"Mismatch in number of skeleton bones: {humanDescription1.skeleton.Length} vs {humanDescription2.skeleton.Length}");
                }

                Array.Sort(humanDescription1.skeleton, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));
                Array.Sort(humanDescription2.skeleton, (a, b) => string.Compare(a.name, b.name, StringComparison.Ordinal));

                for (int i = 0; i < Mathf.Min(humanDescription1.skeleton.Length, humanDescription2.skeleton.Length); i++)
                {
                    var skeleton1 = humanDescription1.skeleton[i];
                    var skeleton2 = humanDescription2.skeleton[i];

                    if (skeleton1.name != skeleton2.name)
                    {
                        Debug.LogWarning($"Skeleton bone mismatch at index {i}:  - Avatar 1 Bone: {skeleton1.name}  - Avatar 2 Bone: {skeleton2.name}");
                    }

                    if (skeleton1.position != skeleton2.position)
                    {
                        Debug.LogWarning($"Position mismatch for bone '{skeleton1.name}':  - Avatar 1: {skeleton1.position}  - Avatar 2: {skeleton2.position}");
                    }

                    if (skeleton1.rotation != skeleton2.rotation)
                    {
                        Debug.LogWarning($"Rotation mismatch for bone '{skeleton1.name}':  - Avatar 1: {skeleton1.rotation}  - Avatar 2: {skeleton2.rotation}");
                    }

                    if (skeleton1.scale != skeleton2.scale)
                    {
                        Debug.LogWarning($"Scale mismatch for bone '{skeleton1.name}':  - Avatar 1: {skeleton1.scale}  - Avatar 2: {skeleton2.scale}");
                    }
                }
            }
            else
            {
                Debug.LogWarning("One or both avatars have no skeleton bones.");
            }

            // Compare additional properties
            Debug.Log("Comparing general properties...");
            if (humanDescription1.armStretch != humanDescription2.armStretch)
            {
                Debug.LogWarning($"Arm stretch mismatch: {humanDescription1.armStretch} vs {humanDescription2.armStretch}");
            }

            if (humanDescription1.legStretch != humanDescription2.legStretch)
            {
                Debug.LogWarning($"Leg stretch mismatch: {humanDescription1.legStretch} vs {humanDescription2.legStretch}");
            }

            if (humanDescription1.upperArmTwist != humanDescription2.upperArmTwist)
            {
                Debug.LogWarning($"Upper arm twist mismatch: {humanDescription1.upperArmTwist} vs {humanDescription2.upperArmTwist}");
            }

            if (humanDescription1.lowerArmTwist != humanDescription2.lowerArmTwist)
            {
                Debug.Log(
                    $"Lower arm twist mismatch: {humanDescription1.lowerArmTwist} vs {humanDescription2.lowerArmTwist}");
            }

            if (humanDescription1.upperLegTwist != humanDescription2.upperLegTwist)
            {
                Debug.LogWarning($"Upper leg twist mismatch: {humanDescription1.upperLegTwist} vs {humanDescription2.upperLegTwist}");
            }

            if (humanDescription1.lowerLegTwist != humanDescription2.lowerLegTwist)
            {
                Debug.LogWarning($"Lower leg twist mismatch: {humanDescription1.lowerLegTwist} vs {humanDescription2.lowerLegTwist}");
            }

            if (humanDescription1.feetSpacing != humanDescription2.feetSpacing)
            {
                Debug.LogWarning($"Feet spacing mismatch: {humanDescription1.feetSpacing} vs {humanDescription2.feetSpacing}");
            }

            if (humanDescription1.hasTranslationDoF != humanDescription2.hasTranslationDoF)
            {
                Debug.LogWarning($"Translation DoF mismatch: {humanDescription1.hasTranslationDoF} vs {humanDescription2.hasTranslationDoF}");
            }
        }

        
        #region Skeleton Bones .........................................................................................

        private static SkeletonBone[] GetSkeletonBones(Transform root, AvatarConfig config)
        {
            List<SkeletonBone> bones = new List<SkeletonBone>();
            
            ProcessBone(root, bones, config);
            
            return bones.ToArray();
        }

        private static void ProcessBone(Transform sourceTransform, List<SkeletonBone> bones, AvatarConfig config)
        {
            SkeletonBone bone = new SkeletonBone
            {
                name = sourceTransform.name,
                position = sourceTransform.localPosition,
                rotation = sourceTransform.localRotation,
                scale = sourceTransform.localScale
            };

            if (config.boneFixes.Find(i => i.transformName == bone.name && i.fixType == AvatarBoneFix.FixType.Exclude) != null)
            {
                // skip it, some bones may mess with rigging
                Debug.LogWarning(sourceTransform + " excluded from Avatar generation.");
            }
            else
            {
                bones.Add(bone);
            }

            // Recursively process each child transform.
            foreach (Transform child in sourceTransform)
            {
                ProcessBone(child, bones, config);
            }
        }

        private static void ApplyBoneFixes(SkeletonBone[] bones, AvatarConfig config)
        {
            for (int i = 0; i < bones.Length; i++)
            {
                SkeletonBone bone = bones[i];
                AvatarBoneFix boneFix = config.boneFixes.Find(i => i.transformName == bone.name);

                if (boneFix != null && boneFix.fixType == AvatarBoneFix.FixType.Rotation && boneFix.RotationOffset != Vector3.zero)
                {
                    if(boneFix.OriginalRotation == Vector3.zero) 
                        boneFix.OriginalRotation = bone.rotation.eulerAngles;
                    
                    
                    bone.rotation.eulerAngles = TransformUtil.AddRotation(boneFix.OriginalRotation, boneFix.RotationOffset);
                    bones[i] = bone;
                    
                    Debug.Log("applying rotation offset " + bone.rotation.eulerAngles  + " to " + bone.name);
                    
                }
            }
        }

        public static bool IsLegBone(AvatarBoneID boneID)
        {
            switch (boneID)
            {
                case AvatarBoneID.LeftUpperLeg:
                case AvatarBoneID.LeftLowerLeg:
                case AvatarBoneID.LeftFoot:
                case AvatarBoneID.LeftToes:
                case AvatarBoneID.RightUpperLeg:
                case AvatarBoneID.RightLowerLeg:
                case AvatarBoneID.RightFoot:
                case AvatarBoneID.RightToes:
                    return true;
                
                default:
                    return false;
            }
        }
        #endregion
    }
}