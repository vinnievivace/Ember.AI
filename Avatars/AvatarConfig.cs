using System;
using System.Collections.Generic;
using Core;
using EmberAI.Attributes;
using EmberAI.Attributes.EmberAI.Attributes;
using EmberAI.Core;
using EmberAI.Core.Util;
using UnityEngine;
using UnityEngine.Serialization;

namespace EmberAI.Avatars
{
    [CreateAssetMenu(fileName = "AvatarConfig", menuName = EmberAISystem.MenuPath + "Settings/AvatarConfig", order = 1)]
public class AvatarConfig : BaseData
{
    [BoxGroup("Settings")] 
    [Tooltip("Attempt to assign the correct Config based on the path / url")]
    public string nameHint;
        
    [BoxGroup("Root")]
    public string rootName;

    [BoxGroup("Root")]
    [Tooltip("Rotate the entire avatar into a T-pose before rigging.")]
    public Vector3 rotationOffset = Vector3.zero;
        
    [BoxGroup("Root")]
    [Tooltip("Y-offset for your CharacterController collider center.")]
    public float yOffset = 0.5f;
        
    [BoxGroup("Bones")]
    [FormerlySerializedAs("bones")] 
    public List<BoneRetargetConfig> BoneMapping;

    [BoxGroup("Bones")]
    [FormerlySerializedAs("BoneReparenting2")] 
    public List<BoneReparentConfig> BoneReparenting;
        
    [BoxGroup("Bones")] 
    public List<BoneRotationConfig> BoneRotations;

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
    public float upperLegTwist = 0.5f;

    [BoxGroup("Rig Settings")]
    [Tooltip("Twist bias for the lower legs (0–1).")]
    public float lowerLegTwist = 0.5f;

    [BoxGroup("Rig Settings")]
    [Tooltip("Foot spacing in meters.")]
    public float feetSpacing = 0f;

    [BoxGroup("Rig Settings")]
    [Tooltip("Enable translation DoF on hips.")]
    public bool hasTranslationDoF = false;

    [BoxGroup("Rig Settings")]
    [Tooltip("Extra local rotation to apply to each shoulder bone (in degrees), to pull the arms out.")]
    public Vector3 shoulderRollOffset = Vector3.zero;

    [ButtonGroup("Debug", "Apply", "At runtime, will rebuild the Avatar and discover any matching instances to apply to.")]
    private void ApplyUpdates()
    {
        if (Application.isPlaying)
        {
            GLBBehaviour target = FindFirstObjectByType<GLBBehaviour>();

            if (target == null)
            {
                Debug.LogError("No GLB Behaviour found in scene");
                
                return;
            }

            string avatarOutputFolder = FileUtil.CombineWithDataPath(AvatarBuilder.OutputFolderName);
            Avatar avatar = AvatarBuilder.Build(target.transform, target.avatarConfig, FileUtil.Combine(avatarOutputFolder, target.avatarConfig.name + ".asset"));
            
            target.SetHumanoidAvatar(avatar, false);
        }
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
        public Vector3 rotation;
        
        [ReadOnly]
        public Vector3 originalRotation;
    }
}