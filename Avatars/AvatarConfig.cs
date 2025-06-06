using System;
using System.Collections.Generic;
using Core;
using EmberAI.Attributes;
using EmberAI.Attributes.EmberAI.Attributes;
using EmberAI.Core;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmberAI.Avatars
{
    [CreateAssetMenu(fileName = "AvatarConfig", menuName = EmberAISystem.MenuPath + "/Settings/AvatarConfig", order = 1)]
    public class AvatarConfig : BaseData
    {
        [BoxGroup("Settings")] 
        [Tooltip("Attempt to assign the correct Config based on the path / url")]
        public string nameHint;
            
        [FormerlySerializedAs("leftFootstep")] [BoxGroup("Audio")]
        public AudioClip footstep;

        [FormerlySerializedAs("rightFootstep")] [BoxGroup("Audio")]
        public AudioClip footstepAlt;

        [BoxGroup("Audio")]
        public AudioClip landJump;

        [BoxGroup("Animation")]
        public AnimationClip idle, walk, run, jumpStart, jumpLand, jumpLandWalk, jumpLandRun, crouch, inAir;
        
        [BoxGroup("Root")]
        public string rootName;

        [BoxGroup("Root")]
        [Tooltip("Rotate the entire avatar into a T-pose before rigging.")]
        public Vector3 rotationOffset = Vector3.zero;
            
        [BoxGroup("Root")]
        [Tooltip("Y-offset for your CharacterController collider center.")]
        public float yOffset = 0.5f;
        
        [BoxGroup("Rig Settings")]
        [Tooltip("How much the arms are allowed to stretch to match your T-pose.")]
        public float armStretch = 0.05f;

        [BoxGroup("Rig Settings")]
        [Tooltip("How much the legs are allowed to stretch.")]
        public float legStretch = 0.05f;

        [BoxGroup("Rig Settings")]
        [Tooltip("Twist bias for the upper arms (0–1).")]
        public float upperArmTwist = 0.5f;

        [BoxGroup("Rig Settings")]
        [Tooltip("Twist bias for the lower arms (0–1).")]
        public float lowerArmTwist = 0.5f;

        [BoxGroup("Rig Settings")]
        [Tooltip("Twist bias for the upper legs (0–1).")]
        public float upperLegTwist = 0.7f;

        [BoxGroup("Rig Settings")]
        [Tooltip("Twist bias for the lower legs (0–1).")]
        public float lowerLegTwist = 1f;

        [BoxGroup("Rig Settings")]
        [Tooltip("Foot spacing in meters.")]
        public float feetSpacing = 0f;

        [BoxGroup("Rig Settings")]
        [Tooltip("Enable translation DoF on hips.")]
        public bool hasTranslationDoF = false;

        [BoxGroup("Rig Settings")]
        [Tooltip("If not Zero, an X Offset for the shoulders")]
        public float shoulderXOffset;
            
        [BoxGroup("Bones")]
        [FormerlySerializedAs("bones")] 
        public List<BoneRetargetConfig> BoneMapping;

        [BoxGroup("Bones")]
        public List<BoneReparentConfig> BoneReparenting;
            
        /*[BoxGroup("Bones"), Tooltip("Runtime Bone Rotation offsets")] 
        public List<BoneRotationConfig> BoneRotations;*/
        
        [ButtonGroup("Debug", "Reset Rig Settings", "Reset Rig settings to HumanDescription defaults")]
        private void ResetRigSettings()
        {
            armStretch = 0.05f;
            legStretch = 0.05f;
            upperArmTwist = 0.5f;
            lowerArmTwist = 0.5f;
            upperLegTwist = 0.7f;
            lowerLegTwist = 1f;
            feetSpacing = 0f;
            hasTranslationDoF = false;
        }
        
        public string GetBoneTarget(AvatarBoneID boneID)
        {
            foreach (BoneRetargetConfig config in BoneMapping)
            {
                if (config.BoneID == boneID)
                {
                    return config.target;
                }
            }
            return null;
        }
    }

    [Serializable]
    public class BoneRetargetConfig
    {
        public AvatarBoneID BoneID;
        public string target;
    }
    
    [Serializable]
    public class BoneReparentConfig
    {
        [FormerlySerializedAs("target")] public string boneName;
        [FormerlySerializedAs("newParent")] public string parentName;
        
        public BoneReparentConfig(string boneName, string parentName)
        {
            this.boneName = boneName;
            this.parentName = parentName;
        }
    }
    
    [Serializable]
    public class BoneRotationConfig
    {
        public AvatarBoneID BoneID;
        public Vector3 offset;
    }
}