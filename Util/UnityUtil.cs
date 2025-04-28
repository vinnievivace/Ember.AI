using System.IO;
using UnityEngine;

namespace EmberAI.Core.Util
{
    /// <summary>
    /// Catch all Class for General Util, things like Layer and Tag helpers etc
    /// </summary>
    public static class UnityUtil
    {
        #region METHODS ////////////////////////////////////////////////////////////////////////////////////////////////

        #region Layers .................................................................................................

        public static int GetLayer(string layerName)
        {
            return LayerMask.NameToLayer(layerName);
        }

        public static void SetLayer(GameObject target, string layerName)
        {
            int layerIndex = GetLayer(layerName);

            if (layerIndex == -1)
            {
                Debug.LogWarning("Layer name not found: " + layerName);
                return;
            }

            target.layer = layerIndex;
        }

        #endregion

        #region Tags ...................................................................................................

        public static bool TagExists(string tag)
        {
            try
            {
                GameObject.FindGameObjectsWithTag(tag);
                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Colors .....................................................................................................

        public static Color ColorFromHex(string hex, float alpha = 1f)
        {
            hex = hex.Replace("#", "");

            if (hex.Length != 6 && hex.Length != 8)
            {
                Debug.LogError("Invalid hex string length. Make sure it's a 6-character or 8-character string.");
                return Color.clear;
            }

            byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);

            if (hex.Length == 8)
            {
                byte a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                alpha = a / 255f;
            }

            return new Color(r / 255f, g / 255f, b / 255f, alpha);
        }


        #endregion

        #region Textures ...........................................................................................

        public static Texture2D CreateTexture(string imagePath)
        {
            if (!FileUtil.FileExists(imagePath))
            {
                Debug.LogError("Unable to Create Texture, imagePath not valid");
                throw new FileNotFoundException(imagePath);
            }

            byte[] fileData = FileUtil.OpenFileAsByteArray(imagePath);

            Texture2D texture = new Texture2D(2, 2);

            texture.LoadImage(fileData);

            return texture;
        }

        #endregion

        #region Materials ..........................................................................................

        #endregion

        #endregion
    }
}