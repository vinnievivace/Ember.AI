using System;
using UnityEngine;

namespace EmberAI.Core.Util
{
    public class CacheUtil
    {
        #region EVENTS /////////////////////////////////////////////////////////////////////////////////////////////////        

        #endregion

        #region ENUMS //////////////////////////////////////////////////////////////////////////////////////////////////

        #endregion

        #region FIELDS /////////////////////////////////////////////////////////////////////////////////////////////////

        public static string CACHE_FOLDER = FileUtil.CombineWithStreamingAssets("Cache");
        
        #endregion

        #region PROPERTIES /////////////////////////////////////////////////////////////////////////////////////////////           

        #endregion
        

        #region General ................................................................................................

        public static void ClearCache() 
        {
            FileUtil.DeleteFolder(CACHE_FOLDER, true);
            
            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif
        }
        
        public static bool IsCached(string folderName, string fileName)
        {
            return FileUtil.FileExists(FileUtil.Combine(CACHE_FOLDER, folderName, fileName));
        }
        
        /// <summary>
        /// Saves the given data to the specified folder and file name within the cache folder.
        /// </summary>
        /// <param name="folderName">The name of the folder within the cache folder where the file will be saved.</param>
        /// <param name="fileName">The name of the file to be saved.</param>
        /// <param name="overwrite">A boolean value indicating whether to overwrite an existing file with the same name.</param>
        /// <param name="data">The byte array containing the data to be saved.</param>
        /// <returns>The full path of the saved file.</returns>
        public static string Save(string folderName, string fileName, bool overwrite, byte[] data)
        {
            string destination = FileUtil.Combine(CACHE_FOLDER, folderName);

            if (!overwrite &&  FileUtil.FileExists(FileUtil.Combine(destination, fileName)))
            {
                fileName = FileUtil.GetNameFromPath(fileName, false) + "_" + DateTime.Now.Ticks + "." + FileUtil.GetExtensionFromName(fileName);
            }

            FileUtil.CreateFolder(destination);
            FileUtil.SaveFile(data, fileName, destination);

            #if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
            #endif

            return FileUtil.Combine(destination, fileName);
        }

        public static string GetFullPath(string folderName, string fileName)
        {
            return FileUtil.Combine(CACHE_FOLDER, folderName, fileName);
        }
        
        public static void UseStreamingAssets()
        {
            CACHE_FOLDER = FileUtil.CombineWithStreamingAssets("Cache");
        }

        public static void UsePersistentDataPath()
        {
            CACHE_FOLDER = FileUtil.CombineWithPersistentDataPath("Cache");
        }

        #endregion

        #region Textures ...............................................................................................

        public static Texture LoadTexture(string folderName, string cacheFileName)
        {
            return LoadTexture(FileUtil.Combine(CACHE_FOLDER, folderName, cacheFileName));
        }

        public static Texture LoadTexture(string path)
        {
            if(FileUtil.FileExists(path))
            {
                try
                {
                    using (var stream = FileUtil.OpenFile(path))
                    {
                        var buffer = new byte[stream.Length];
                        stream.Read(buffer, 0, (int)stream.Length);
                        var texture = new Texture2D(1, 1);
                        texture.LoadImage(buffer);
                        return texture;
                    }    
                }
                catch (UnityException e)
                {
                    Debug.LogError($"Error loading texture: {e.Message}");
                }
                
            }

            return null;
        }
        
        #endregion
    }
}
