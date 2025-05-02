using System;
using System.Collections.Generic;
using EmberAI.Core;
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

        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion

        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        private static bool IsValid(Transform target, AvatarConfig config)
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

            foreach (AvatarBoneConfig boneConfig in config.bones)
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

        public static Avatar Build(Transform target, AvatarConfig config, string assetPath)
        {
            if (!IsValid(target, config))
            {
                Debug.LogError("Cannot build Avatar. The target or config is invalid.");
                return null;
            }
            
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
            
            var skeletonBones = GetSkeletonBones(root);
            
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
            UnityEditor.AssetDatabase.CreateAsset(avatar, assetPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();
            #endif
            
            return avatar;
        }

        private static void DebugConfig(Transform target, AvatarConfig config)
        {
            if (target == null)
            {
                Debug.LogError("Target Transform is null. Cannot debug avatar.");
                return;
            }

            if (config == null || config.bones == null || config.bones.Count == 0)
            {
                Debug.LogError("AvatarConfig or its bones list is null or empty. Cannot debug avatar.");
                return;
            }

            foreach (AvatarBoneConfig boneConfig in config.bones)
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
        }
        
        #region Skeleton Bones .........................................................................................

        private static SkeletonBone[] GetSkeletonBones(Transform root)
        {
            List<SkeletonBone> bones = new List<SkeletonBone>();
            
            ProcessBone(root, bones);
            
            return bones.ToArray();
        }

        private static void ProcessBone(Transform sourceTransform, List<SkeletonBone> bones)
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
                ProcessBone(child, bones);
            }
        }

        #endregion

        #endregion
    }
}