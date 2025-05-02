using System;
using System.Collections.Generic;
using UnityEngine;

namespace EmberAI.Avatars
{
    [CreateAssetMenu(fileName = "AvatarConfig", menuName = "BOMB/AvatarConfig", order = 1)]
    public class AvatarConfig : ScriptableObject
    {
        public enum RiggingType { AlteredState, CharacterCreator }
        
        public RiggingType riggingType;
        
        public string rootName;

        [Tooltip("On instantiation the offset will be applied to the Avatars transform (local rotation)")]
        public Quaternion rotationOffset;
        
        [Tooltip("On instantiation the y offset will be applied to the associated CharacterControllers collider center, to ensure its grounded")]
        public float yOffset = 0.5f;
        
        public List<AvatarBoneConfig> bones = new List<AvatarBoneConfig>();
        
        public void ResetBones()
        {
            bones.Clear();

            foreach (var map in GetBoneMapping(riggingType))
            {
                bones.Add(new AvatarBoneConfig
                {
                    BoneID = map.Key,
                    target = map.Value
                });
            }
        }
        
        Dictionary<AvatarBoneID, string> GetBoneMapping(RiggingType type)
        {
            Dictionary<AvatarBoneID, string> boneMapping;

            if (type == RiggingType.AlteredState)
            {
                rootName = "root";
                
                boneMapping = new Dictionary<AvatarBoneID, string>
                {
                    { AvatarBoneID.Hips, "root.x" },
                    { AvatarBoneID.Spine, "spine_01.x" },
                    { AvatarBoneID.Chest, "spine_02.x" },
                    { AvatarBoneID.UpperChest, "spine_03.x" },
                    { AvatarBoneID.Neck, "neck.x" },
                    { AvatarBoneID.Head, "head.x" },
                    { AvatarBoneID.LeftShoulder, "shoulder.l" },
                    { AvatarBoneID.LeftUpperArm, "arm_stretch.l" },
                    { AvatarBoneID.LeftLowerArm, "forearm_stretch.l" },
                    { AvatarBoneID.LeftHand, "hand.l" },
                    { AvatarBoneID.RightShoulder, "shoulder.r" },
                    { AvatarBoneID.RightUpperArm, "arm_stretch.r" },
                    { AvatarBoneID.RightLowerArm, "forearm_stretch.r" },
                    { AvatarBoneID.RightHand, "hand.r" },
                    { AvatarBoneID.LeftUpperLeg, "thigh_stretch.l" },
                    { AvatarBoneID.LeftLowerLeg, "leg_stretch.l" },
                    { AvatarBoneID.LeftFoot, "foot.l" },
                    { AvatarBoneID.LeftToes, "toes_01.l" },
                    { AvatarBoneID.RightUpperLeg, "thigh_stretch.r" },
                    { AvatarBoneID.RightLowerLeg, "leg_stretch.r" },
                    { AvatarBoneID.RightFoot, "foot.r" },
                    { AvatarBoneID.RightToes, "toes_01.r" }
                    
                };
            }
            else if (type == RiggingType.CharacterCreator)
            {
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
            }
            else
            {
                throw new ArgumentException("Unsupported rigging type: " + type);
            }

            return boneMapping;
        }


    }

    [Serializable]
    public class AvatarBoneConfig
    {
        public AvatarBoneID BoneID;
        public string target;
    }
    
}