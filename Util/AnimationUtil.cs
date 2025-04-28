using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace EmberAI.Core.Util
{
    public static class AnimationUtil
    {
        #region EDITOR .................................................................................................

        /// <summary>
        /// Editor only method for returning all <see cref="AnimationClip"/>s discovered in the supplied folder name (within the local projects Assets folder)
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static List<AnimationClip> GetAnimationClips(string folderPath)
        {
            List<AnimationClip> animationClips = new List<AnimationClip>();

            if (string.IsNullOrWhiteSpace(folderPath))
            {
                return animationClips;
            }

#if UNITY_EDITOR

            string[] assetGUIDs = UnityEditor.AssetDatabase.FindAssets("t:AnimationClip", new[] { folderPath });

            foreach (string guid in assetGUIDs)
            {
                string assetPath = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                AnimationClip clip = UnityEditor.AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);

                if (clip != null)
                {
                    animationClips.Add(clip);
                }
            }
#endif

            return animationClips;
        }

        /// <summary>
        /// Editor only method for returning all <see cref="AnimationClip"/>s discovered in the supplied prefab
        /// </summary>
        /// <param name="folderPath"></param>
        /// <returns></returns>
        public static List<AnimationClip> GetAnimationClips(GameObject prefab)
        {
            List<AnimationClip> animationClips = new List<AnimationClip>();

            if (prefab == null) return animationClips;

#if UNITY_EDITOR

            string assetPath = UnityEditor.AssetDatabase.GetAssetPath(prefab);

            Object[] allAssets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (Object asset in allAssets)
            {
                if (asset is AnimationClip clip)
                {
                    animationClips.Add(clip);
                }
            }

#endif

            return animationClips;
        }

        #endregion
    }
}