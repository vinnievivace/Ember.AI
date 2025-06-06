using System;
using System.Collections.Generic;
using EmberAI.Core;
using EmberAI.Core.Util;
using UnityEngine;

namespace EmberAI.Avatars
{
    /// <summary>
    /// Util to prepare avatar GLB for Humanoid rigging / Animation systems by applying Skeleton remapping.
    /// </summary>
    public static class AvatarBuilder
    {
        #region FIELDS

        /// <summary>
        /// Name of folder (inside StreamingAssets) where runtime generated avatars are stored.
        /// </summary>
        public const string OutputFolderName = "Avatars";

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Builds a Unity Humanoid Avatar from a runtime-generated model hierarchy.
        /// </summary>
        /// <param name="target">Root transform of the imported model.</param>
        /// <param name="config">AvatarConfig containing bone mappings and rig settings.</param>
        /// <param name="assetPath">Path (in StreamingAssets) to save the generated Avatar asset.</param>
        /// <returns>The created Avatar, or null on failure.</returns>
        public static void Build(Transform target, AvatarConfig config, string assetPath, Action<Avatar> onComplete)
        {
            // Validate inputs and mapping
            if (!IsValid(target, config))
            {
                Debug.LogError("Cannot build Avatar. The target or config is invalid.");
                
                return;
            }

            ApplyBoneReparenting(target, config);
            //ApplyBoneRotations(target, config);

            // Store original transform
            Vector3 originalPosition = target.position;
            Quaternion originalRotation = target.rotation;

            // Reset and apply user-defined T-pose rotation offset
            target.ResetLocalTransform();
            target.localRotation = Quaternion.Euler(config.rotationOffset);

            // Prepare HumanDescription
            HumanDescription humanDescription = new HumanDescription();
            Transform root = string.IsNullOrEmpty(config.rootName)
                ? target
                : target.FindChildTransform(config.rootName);

            if (root == null)
                throw new Exception("Root transform not found in target hierarchy. looking for " + config.rootName);

            // Map each BoneRetargetConfig to a HumanBone entry
            var humanBones = new HumanBone[config.BoneMapping.Count];
            for (int i = 0; i < config.BoneMapping.Count; i++)
            {
                var boneConfig = config.BoneMapping[i];
                var boneTransform = root.FindChildTransform(boneConfig.target);
                if (boneTransform == null)
                {
                    Debug.LogError($"Bone '{boneConfig.BoneID}' (target '{boneConfig.target}') not found.");
                    return;
                }

                humanBones[i] = new HumanBone
                {
                    boneName = boneConfig.target,
                    humanName = boneConfig.BoneID.ToString(),
                    limit = new HumanLimit { useDefaultValues = true }
                };
            }

            // Build the skeleton list
            SkeletonBone[] skeletonBones = GetSkeleton(root);

            // Assign rig parameters from config
            humanDescription.human              = humanBones;
            humanDescription.skeleton           = skeletonBones;
            humanDescription.armStretch         = config.armStretch;
            humanDescription.legStretch         = config.legStretch;
            humanDescription.upperArmTwist      = config.upperArmTwist;
            humanDescription.lowerArmTwist      = config.lowerArmTwist;
            humanDescription.upperLegTwist      = config.upperLegTwist;
            humanDescription.lowerLegTwist      = config.lowerLegTwist;
            humanDescription.feetSpacing        = config.feetSpacing;
            humanDescription.hasTranslationDoF  = config.hasTranslationDoF;

            if (config.shoulderXOffset != 0) ApplyShoulderOffset(root, config);

            // Create and save the Avatar
            Avatar avatar;
            try
            {
                avatar = UnityEngine.AvatarBuilder.BuildHumanAvatar(root.gameObject, humanDescription);
            }
            catch (Exception e)
            {
                throw new Exception("Avatar creation failed. Please check bone mappings and hierarchy.", e);
            }

            if (!avatar.isValid || !avatar.isHuman)
            {
                Debug.LogError("Avatar creation failed. Please check bone mappings and hierarchy.");
                return;
            }

            // Persist asset if in Editor
#if UNITY_EDITOR
            string directoryPath = System.IO.Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(directoryPath))
                System.IO.Directory.CreateDirectory(directoryPath);

            UnityEditor.AssetDatabase.CreateAsset(avatar, FileUtil.GetAssetPath(assetPath));
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
#endif

            // Restore original transform
            target.position = originalPosition;
            target.rotation = originalRotation;

            Debug.Log($"Avatar successfully created and set for the target at '{assetPath}'.");
            
            onComplete?.Invoke(avatar);
        }

        #endregion

        #region PRIVATE HELPERS

        private static bool IsValid(Transform target, AvatarConfig config)
        {
            const string prefix = "[AvatarValidator]";
            if (target == null)
            {
                Debug.LogError($"{prefix} Target Transform is null.");
                return false;
            }

            if (config == null)
            {
                Debug.LogError($"{prefix} AvatarConfig is null.");
                return false;
            }

            if (string.IsNullOrEmpty(config.rootName))
            {
                Debug.LogError($"{prefix} config.rootName is null or empty.");
                return false;
            }

            if (config.BoneMapping == null || config.BoneMapping.Count == 0)
            {
                Debug.LogError($"{prefix} BoneMapping list is null or empty.");
                return false;
            }

            // Find and cache bones
            Transform root = FindChildRecursively(target, config.rootName, false);
            if (root == null)
            {
                Debug.LogError($"{prefix} Root '{config.rootName}' not found under '{target.name}'.");
                return false;
            }

            var allBones = root.GetComponentsInChildren<Transform>(false);
            var lookup = new Dictionary<string, Transform>(allBones.Length);
            foreach (var t in allBones)
            {
                if (!lookup.ContainsKey(t.name))
                    lookup[t.name] = t;
            }

            foreach (var map in config.BoneMapping)
            {
                if (string.IsNullOrEmpty(map.target) || !lookup.ContainsKey(map.target))
                {
                    Debug.LogError($"{prefix} Missing bone '{map.target}' for ID {map.BoneID}." );
                    return false;
                }
            }

            return true;
        }

        private static Transform FindChildRecursively(Transform parent, string name, bool includeInactive)
        {
            if (parent.name.Equals(name, StringComparison.Ordinal))
                return parent;

            foreach (Transform child in parent)
            {
                if (!includeInactive && !child.gameObject.activeInHierarchy)
                    continue;

                var found = FindChildRecursively(child, name, includeInactive);
                if (found != null) return found;
            }

            return null;
        }

        private static SkeletonBone[] GetSkeleton(Transform root)
        {
            var bones = new List<SkeletonBone>();
            ApplyBoneRetargeting(root, bones);
            return bones.ToArray();
        }

        private static void ApplyBoneRetargeting(Transform current, List<SkeletonBone> bones)
        {
            bones.Add(new SkeletonBone
            {
                name     = current.name,
                position = current.localPosition,
                rotation = current.localRotation,
                scale    = current.localScale
            });
            foreach (Transform child in current)
                ApplyBoneRetargeting(child, bones);
        }

        private static void ApplyBoneReparenting(Transform target, AvatarConfig config)
        {
            foreach (var rep in config.BoneReparenting)
            {
                var bone = target.FindChildTransform(rep.boneName);
                var parent = target.FindChildTransform(rep.parentName);
                if (bone == null || parent == null)
                {
                    Debug.LogError($"Reparenting failed: '{rep.boneName}' or '{rep.parentName}' missing.");
                    continue;
                }
                bone.SetParent(parent);
            }
        }
        
        /*public static void ApplyBoneRotations(Transform target, AvatarConfig config)
        {
            // store original rotation for each mapped bone
            foreach (BoneRetargetConfig boneRetarget in config.BoneMapping)
            {
                BoneRotationConfig boneRotation = config.BoneRotations.Find(x => x.BoneID == boneRetarget.BoneID);

                if (boneRotation == null)
                {
                    boneRotation = new BoneRotationConfig {BoneID = boneRetarget.BoneID};
                    
                    config.BoneRotations.Add(boneRotation);
                }
                
                Transform bone = target.FindChildTransform(boneRetarget.target);

                if (boneRotation.originalRotation == Vector3.zero)
                {
                    boneRotation.rotation = bone.localEulerAngles;
                    boneRotation.originalRotation = bone.localEulerAngles;
                }
                
            }
            
            foreach (BoneRotationConfig boneRotation in config.BoneRotations)
            {
                BoneRetargetConfig map = config.BoneMapping.Find(x => x.BoneID == boneRotation.BoneID);
                
                if (map == null) throw new Exception($"Bone rotation {boneRotation.BoneID} has no mapping.");

                Transform bone = target.FindChildTransform(map.target);
                
                if (bone == null)
                {
                    Debug.LogError($"Missing bone '{map.target}' for rotation {boneRotation.BoneID}.");
                    continue;
                }
                
                if(boneRotation.originalRotation == Vector3.zero) boneRotation.originalRotation = bone.localEulerAngles;
                
                bone.localEulerAngles = boneRotation.rotation;
            }
        }*/

        public static void ApplyShoulderOffset(Transform root, AvatarConfig config)
        {
            string leftMap  = config.BoneMapping.Find(b => b.BoneID == AvatarBoneID.LeftShoulder)?.target;
            string rightMap = config.BoneMapping.Find(b => b.BoneID == AvatarBoneID.RightShoulder)?.target;
            
            if (!string.IsNullOrEmpty(leftMap))
            {
                Transform t = root.FindChildTransform(leftMap);
                
                if (t != null) t.localPosition = new Vector3(0 - config.shoulderXOffset, t.localPosition.y, t.localPosition.z);;
            }
            
            if (!string.IsNullOrEmpty(rightMap))
            {
                Transform t = root.FindChildTransform(rightMap);
                
                if (t != null) t.localPosition = new Vector3(config.shoulderXOffset, t.localPosition.y, t.localPosition.z);;
            }
        }

        #endregion
    }
}
