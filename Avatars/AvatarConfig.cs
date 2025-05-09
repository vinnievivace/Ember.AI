using System;
using System.Collections.Generic;
using Core;
using EmberAI.Attributes;
using EmberAI.Core;
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
        [Tooltip("On instantiation the offset will be applied to the Avatars transform (local rotation)")]
        public Quaternion rotationOffset;
        
        [BoxGroup("Root")]
        [Tooltip("On instantiation the y offset will be applied to the associated CharacterControllers collider center, to ensure its grounded")]
        public float yOffset = 0.5f;
        
        [BoxGroup("Bones")]
        [FormerlySerializedAs("bones")] 
        public List<BoneRetargetConfig> BoneMapping;

        [FormerlySerializedAs("BoneReparenting2")] [BoxGroup("Bones")] 
        public List<BoneReparentConfig> BoneReparenting;
        
        [BoxGroup("Bones")] 
        public List<BoneRotationConfig> BoneRotations;

        public override void Initialize()
        {
            base.Initialize();

            //
            
        }

        /// <summary>
        /// TODO - CC mapping, then delete this
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        [Obsolete("Dont use", true)]
        Dictionary<AvatarBoneID, string> GetBoneMapping()
        {
            // generate a config for Character creator then kill this
            Dictionary<AvatarBoneID, string> boneMapping;

            
                rootName = "RL_BoneRoot";
                
                boneMapping = new Dictionary<AvatarBoneID, string>
                {
                    { AvatarBoneID.Hips, "root_x" },
                    { AvatarBoneID.LeftUpperLeg, "thigh_stretch_l" },
                    { AvatarBoneID.RightUpperLeg, "thigh_stretch_r" },
                    { AvatarBoneID.LeftLowerLeg, "leg_stretch_l" },
                    { AvatarBoneID.RightLowerLeg, "leg_stretch_r" },
                    { AvatarBoneID.LeftFoot, "foot_l" },
                    { AvatarBoneID.RightFoot, "foot_r" },
                    
                    { AvatarBoneID.Spine, "spine_01_x" },
                    { AvatarBoneID.Chest, "spine_02_x" },
                    { AvatarBoneID.Neck, "neck_x" },
                    { AvatarBoneID.Head, "head_x" },
                    
                    { AvatarBoneID.LeftShoulder, "shoulder_l" },
                    { AvatarBoneID.RightShoulder, "shoulder_r" },
                    { AvatarBoneID.LeftUpperArm, "arm_stretch_l" },
                    { AvatarBoneID.RightUpperArm, "arm_stretch_r" },
                    { AvatarBoneID.LeftLowerArm, "forearm_stretch_l" },
                    { AvatarBoneID.RightLowerArm, "forearm_stretch_r" },
                    { AvatarBoneID.LeftHand, "hand_l" },
                    { AvatarBoneID.RightHand, "hand_r" },
                    { AvatarBoneID.LeftToes, "toes_01_l" },
                    { AvatarBoneID.RightToes, "toes_01_r" },
                    { AvatarBoneID.LeftEye , "c_eye_offset_l" },
                    { AvatarBoneID.RightEye, "c_eye_offset_r"},
                    { AvatarBoneID.Jaw, "c_jawbone_x"},
                    { AvatarBoneID.UpperChest, "spine_03_x" }
                };
            
            

            return boneMapping;
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
    }
}