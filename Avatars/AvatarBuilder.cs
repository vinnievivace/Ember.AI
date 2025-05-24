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
        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Name of folder (inside StreamingAssets) where runtime generated avatars are stored.
        /// </summary>
        public const string OutputFolderName = "Avatars";
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// Checks that:
        /// 1) target & config exist
        /// 2) config.rootName is valid and actually found
        /// 3) each BoneRetargetConfig.target is non‐empty and present under that root
        /// </summary>
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

            // 1) find the root once
            Transform root = FindChildRecursively(target, config.rootName, includeInactive: false);
            if (root == null)
            {
                Debug.LogError($"{prefix} Root '{config.rootName}' not found under '{target.name}'. [CONFIG] " + config.name);
                return false;
            }

            // 2) cache all children under root
            var allBones = root.GetComponentsInChildren<Transform>(includeInactive: false);
            var boneLookup = new Dictionary<string, Transform>(allBones.Length);
            foreach (var t in allBones)
            {
                // only keep the first occurrence if there are duplicates
                if (!boneLookup.ContainsKey(t.name))
                    boneLookup[t.name] = t;
            }

            // 3) validate every mapping
            foreach (var boneMap in config.BoneMapping)
            {
                if (string.IsNullOrEmpty(boneMap.target))
                {
                    Debug.LogWarning($"{prefix} BoneID {boneMap.BoneID} has an empty target name.");
                    return false;
                }

                if (!boneLookup.ContainsKey(boneMap.target))
                {
                    Debug.LogError(
                        $"{prefix} Missing bone '{boneMap.target}' (BoneID {boneMap.BoneID}) under root '{config.rootName}'."
                    );
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Depth‐first search for a child with the given name.
        /// Honors includeInactive when descending.
        /// </summary>
        private static Transform FindChildRecursively(Transform parent, string name, bool includeInactive)
        {
            if (parent.name.Equals(name, StringComparison.Ordinal))
                return parent;

            foreach (Transform child in parent)
            {
                if (!includeInactive && !child.gameObject.activeInHierarchy)
                    continue;

                var found = FindChildRecursively(child, name, includeInactive);
                if (found != null)
                    return found;
            }

            return null;
        }

        public static Avatar Build(Transform target, AvatarConfig config, string assetPath)
        {
            if (!IsValid(target, config))
            {
                Debug.LogError("Cannot build Avatar. The target or config is invalid.");
                return null;
            }
            
            ApplyBoneReparenting(target, config);
            ApplyBoneRotations(target, config);
            
            // store position and rotation to allow reset once Avatar is build
            // this is important to ensure Avatar builds correctly
            Vector3 originalPosition = target.position;
            Quaternion originalRotation = target.rotation;
            
            target.ResetLocalTransform();
            
            HumanDescription humanDescription = new HumanDescription();
            Transform root = (config.rootName == "") ? target : target.FindChildTransform(config.rootName);
            
            if(root == null) throw new Exception("Root transform not found in target hierarchy. looking for " + config.rootName);
            
            var humanBones = new HumanBone[config.BoneMapping.Count];
            int index = 0;
            
            foreach (BoneRetargetConfig boneConfig in config.BoneMapping)
            {
                Transform boneTransform = root.FindChildTransform(boneConfig.target);

                if (boneTransform == null)
                {
                    Debug.LogError($"Bone '{boneConfig.BoneID}' (target transform: '{boneConfig.target}') not found in target hierarchy. Root = " + root.name);

                    return null;
                }

                humanBones[index] = new HumanBone
                {
                    boneName = boneConfig.target,
                    humanName = boneConfig.BoneID.ToString(),
                    limit = new HumanLimit { useDefaultValues = true }
                };

                
                
                index++;
            }
            
            var skeletonBones = GetSkeleton(root);
            
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
            
            Avatar avatar;
           
            try
            {
                avatar = SaveAvatar(root.gameObject, humanDescription, assetPath);
            }
            catch (Exception e)
            {
                throw new Exception($"Avatar creation failed. Please check bone mappings and hierarchy.", e);
            }

            target.position = originalPosition;
            target.rotation = originalRotation;

            Debug.Log($"Avatar successfully created and set for the target at '{assetPath}'.");
            
            return avatar;
        }

        private static Avatar SaveAvatar(GameObject target, HumanDescription humanDescription, string assetPath)
        {
            Avatar avatar;
           
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
            
            // Ensure the directory exists
            string directoryPath = System.IO.Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrEmpty(directoryPath))
            {
                System.IO.Directory.CreateDirectory(directoryPath);
            }
            
            // if running in the Editor, we save the Avatar. 
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.CreateAsset(avatar, FileUtil.GetAssetPath(assetPath));
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            #endif
            
            return avatar;
        }

        /*private static void DebugConfig(Transform target, AvatarConfig config)
        {
            if (target == null)
            {
                Debug.LogError("Target Transform is null. Cannot debug avatar.");
                return;
            }

            if (config == null || config.BoneMapping == null || config.BoneMapping.Count == 0)
            {
                Debug.LogError("AvatarConfig or its bones list is null or empty. Cannot debug avatar.");
                return;
            }

            foreach (BoneRetargetConfig boneConfig in config.BoneMapping)
            {
                // Find the target transform for the current bone
                Transform boneTransform = target.FindChildTransform(boneConfig.target);
                if (boneTransform == null)
                {
                    Debug.LogWarning($"Couldn't find transform '{boneConfig.target}' for bone '{boneConfig.BoneID}'.");
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
        }*/
        
        #region Skeleton Bones .........................................................................................

        private static SkeletonBone[] GetSkeleton(Transform root)
        {
            List<SkeletonBone> bones = new List<SkeletonBone>();
            
            ApplyBoneRetargeting(root, bones);
            
            return bones.ToArray();
        }

        private static void ApplyBoneRetargeting(Transform sourceTransform, List<SkeletonBone> bones)
        {
            SkeletonBone bone = new SkeletonBone
            {
                name = sourceTransform.name,
                position = sourceTransform.localPosition,
                rotation = sourceTransform.localRotation,
                scale = sourceTransform.localScale
            };

            bones.Add(bone);
            
            // Recursively process each child transform.
            foreach (Transform child in sourceTransform)
            {
                ApplyBoneRetargeting(child, bones);
            }
        }
        
        private static void ApplyBoneRotations(Transform target, AvatarConfig config)
        {
            foreach (BoneRotationConfig rotationConfig in config.BoneRotations)
            {
                // get bone mapping to apply rotation to correct transform
                BoneRetargetConfig boneRetargetMap = config.BoneMapping.Find(x => x.BoneID == rotationConfig.BoneID);
                
                if(boneRetargetMap == null) throw new Exception($"Bone rotation {rotationConfig.BoneID} has no matching bone mapping.");
                
                Transform boneTransform = target.FindChildTransform(boneRetargetMap.target);
                
                if (boneTransform == null)
                {
                    Debug.LogError($"Target Transform is missing required child: {boneRetargetMap.target} for bone {boneRetargetMap.BoneID}.");
                    return;
                }
                
                boneTransform.localEulerAngles = boneTransform.localEulerAngles + rotationConfig.rotation;
            }
        }

        private static void ApplyBoneReparenting(Transform target, AvatarConfig config)
        {
            foreach (BoneReparentConfig reparentConfig in config.BoneReparenting)
            {
                Transform boneTransform = target.FindChildTransform(reparentConfig.boneName);
                Transform targetParent = target.FindChildTransform(reparentConfig.parentName);
                
                if (boneTransform == null)
                {
                    Debug.LogError($"Target Transform is missing required child: {reparentConfig.boneName}.");
                    return;
                }

                if (targetParent == null)
                {
                    Debug.LogError($"Target Transform is missing required parent: {reparentConfig.parentName}.");
                    return;
                }
                
                Debug.Log("Reparent " + boneTransform.name + " to " + targetParent.name);
                
                
                boneTransform.SetParent(targetParent);
            }
        }

        #endregion

        #endregion
    }
}