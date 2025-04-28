using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace EmberAI.Core.Util
{
    public static class FileUtil
    {
        #region Files ..................................................................................................

        /// <summary>
        /// Returns a <see cref="DateTime"/> representing the latest write time for the file at the supplied path;
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DateTime GetFileTimeStamp(string path)
        {
            if (FileExists(path))
            {
                return File.GetLastWriteTime(path);
            }

            Debug.LogError("Unable to GetFileTimeStamp, no file exists at " + path);

            return DateTime.Now;
        }


        /// <summary>
        /// Splits the path by path separator and returns the final entry
        /// </summary>
        /// <param name="path"></param>
        /// <param name="includeExtension"></param>
        /// <returns></returns>
        public static string GetNameFromPath(string path, bool includeExtension = true)
        {
            path = GetPlatformSpecificPath(path);
            var splitString = path.Split(new char[] { GetApplicationSpecificPathSeparator() });
            if (splitString.Length <= 0)
            {
                return path;
            }

            var name = splitString[splitString.Length - 1];
            if (!includeExtension && name.Contains("."))
            {
                name = name.Substring(0, name.LastIndexOf('.'));
            }

            return name;
        }

        /// <summary>
        /// Returns the extension from a given string
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string GetExtensionFromName(string name)
        {
            var splitString = name.Split(new char[] { '.' });
            if (splitString.Length <= 0)
            {
                return "";
            }
            return splitString[splitString.Length - 1];
        }

        public static string GetFolderFromPath(string path)
        {
            path = GetPlatformSpecificPath(path);
            var lastIndex = path.LastIndexOf(GetApplicationSpecificPathSeparator());

            return path.Substring(0, lastIndex);
        }

        /// <summary>
        /// Returns the human-readable file size for an arbitrary, 64-bit file size 
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public static string GetBytesReadable(int i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return $"< 1KB"; // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.# ") + suffix;
        }

        public static bool FileExists(string path)
        {
            path = GetPlatformSpecificPath(path);
            if (!string.IsNullOrWhiteSpace(path))
            {
                return File.Exists(path);
            }
            return false;
        }

        /// <summary>
        /// Write text to File, either overriding or appending any existing file, or creating it if it doesn't exist
        /// </summary>
        /// <param name="data"></param>
        /// <param name="filename"></param>
        /// <param name="destinationPath"></param>
        /// <param name="overrideExisting">When true, any existing file will be overwritten, when false, text is appended</param>
        public static void WriteToFile(string data, string filename, string destinationPath, bool overrideExisting = false)
        {
            destinationPath = GetPlatformSpecificPath(destinationPath);

            CreateFolder(destinationPath);

            if (overrideExisting)
            {
                File.WriteAllText(Path.Combine(destinationPath, filename), data);
            }
            else
            {
                data = Environment.NewLine + data;

                File.AppendAllText(Path.Combine(destinationPath, filename), data);
            }

        }

        /// <summary>
        /// Saves a (plain text) File to the supplied destination/filename
        /// </summary>
        /// <param name="data"></param>
        /// <param name="destinationPath"></param>
        /// <param name="filename"></param>
        public static void SaveFile(string data, string filename, string destinationPath, bool overrideExisting = true)
        {
            destinationPath = GetPlatformSpecificPath(destinationPath);

            if (FileExists(Path.Combine(destinationPath, filename)) && !overrideExisting) return;

            CreateFolder(destinationPath);

            File.WriteAllText(Path.Combine(destinationPath, filename), data);

        }

        /// <summary>
        /// Saves a File to the supplied destination/filename.
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="filename"></param>
        /// <param name="destinationPath"></param>
        /// <param name="disposeStream">Determine if the the supplied FileStream should be disposed of once it has been saved. Recommended!</param>
        public static void SaveFile(FileStream fileData, string filename, string destinationPath, bool disposeStream)
        {
            var info = new DirectoryInfo(destinationPath);

            if (!info.Exists)
            {
                info.Create();
            }

            var path = Path.Combine(destinationPath, filename);
            path = GetPlatformSpecificPath(path);

            using (var outputFileStream = new FileStream(path, FileMode.Create))
            {
                fileData.CopyTo(outputFileStream);
            }

            if (disposeStream)
            {
                fileData.Dispose();
            }
        }

        /// <summary>
        /// Saves a File to the supplied destination/filename.
        /// </summary>
        /// <param name="fileData"></param>
        /// <param name="filename"></param>
        /// <param name="destinationPath"></param>
        public static void SaveFile(byte[] fileData, string filename, string destinationPath)
        {
            if(fileData == null) throw new Exception("no fileData, cannot save");
            if(filename.IsEmptyString()) throw new Exception("no fileName, cannot save");
            if(destinationPath.IsEmptyString()) throw new Exception("no destination path, cannot save");
            
            var info = new DirectoryInfo(destinationPath);

            if (!info.Exists)
            {
                info.Create();
            }

            var path = Path.Combine(destinationPath, filename);
            path = GetPlatformSpecificPath(path);

            try
            {
                File.WriteAllBytes(path, fileData);
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
                Debug.LogError(path);
            }
            
        }

        /// <summary>
        /// Gets a file as a <see cref="FileStream"/> from the supplied path. Remember to dispose of the stream once
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static FileStream OpenFile(string path)
        {
            path = GetPlatformSpecificPath(path);
            var info = new FileInfo(path);

            if (info.Exists)
            {
                return File.OpenRead(path);
            }

            Debug.LogError("Unable to open File at path " + path);

            return null;
        }
        
        public static string OpenTextFile(string path)
        {
            path = GetPlatformSpecificPath(path);
            var info = new FileInfo(path);

            if (info.Exists)
            {
                return File.OpenText(path).ReadToEnd();
            }

            Debug.LogError("Unable to open File at path " + path);

            return null;
        }

        /// <summary>
        /// Gets a file as a byte array from the supplied path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static byte[] OpenFileAsByteArray(string path)
        {
            path = GetPlatformSpecificPath(path);
            var stream = OpenFile(path);

            if (stream == null) return null;

            return GetByteArrayFromStream(stream);
        }
        
        public static void CopyFile(string sourcePath, string destinationPath, bool overrideExisting)
        {
            File.Copy(sourcePath, destinationPath, overrideExisting);
        }

        /// <summary>
        /// Deletes File at supplied path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool DeleteFile(string path)
        {
            if (FileExists(path))
            {
                File.Delete(path);
                
                var meta = path + ".meta";
                
                if(FileExists(meta)) File.Delete(meta);

                return true;
            }
            Debug.LogWarning(path + " not found, unable to Delete");

            return false;
        }
        
        public static void MoveFile(string sourceFolder, string destinationFolder, string targetFile, bool overrideIfExists)
        {
            var sourcePath = GetPlatformSpecificPath(Path.Combine(sourceFolder, targetFile));
            var destinationPath = GetPlatformSpecificPath(Path.Combine(destinationFolder, targetFile));

            if (!FileExists(sourcePath)) { Debug.LogError(sourcePath + " not a valid path"); return; }

            if (sourceFolder == destinationFolder) { Debug.LogError("source and destination folders are identical"); return; }

            if (FileExists(destinationPath))
            {
                if (overrideIfExists) { DeleteFile(destinationPath); }
                else { Debug.LogError(destinationPath + " already exists"); return; }
            }

            CreateFolder(destinationFolder);

            File.Move(sourcePath, destinationPath);
        }

        #endregion

        #region Folders ................................................................................................

        public static void CreateFolder(string path)
        {
            if (FolderExists(path)) return;

            path = GetPlatformSpecificPath(path);

            Directory.CreateDirectory(path);
        }

        public static bool FolderExists(string path)
        {
            path = GetPlatformSpecificPath(path);

            return !string.IsNullOrWhiteSpace(path) && Directory.Exists(path);
        }

        /// <summary>
        /// Copies a folder and its contents to another folder (which is created if it does not exist), optionally override
        /// if already exists.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="overrideExisting"></param>
        public static void CopyFolder(string source, string target, bool overrideExisting = true)
        {
            var sourceInfo = new DirectoryInfo(source);
            var targetInfo = new DirectoryInfo(target);

            if (!sourceInfo.Exists)
            {
                Debug.LogError("Unable to copy folder " + source + " as it does not exist");
                return;
            }

            if (!targetInfo.Exists)
            {
                Directory.CreateDirectory(targetInfo.FullName);
            }

            // Copy each file into the new folder.
            foreach (var file in sourceInfo.GetFiles())
            {
                var sourcePath = GetPlatformSpecificPath(Path.Combine(sourceInfo.FullName, file.Name));
                var destinationPath = GetPlatformSpecificPath(Path.Combine(targetInfo.FullName, file.Name));

                if (file.Extension != ".meta")
                {
                    if (!FileExists(destinationPath) || overrideExisting)
                    {
                        const int bufferSize = 1024 * 1024;

                        using (var destinationData = new FileStream(destinationPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
                        {
                            using (var sourceData = new FileStream(sourcePath, FileMode.Open, FileAccess.ReadWrite))
                            {
                                destinationData.SetLength(sourceData.Length);

                                var bytesRead = -1;
                                var bytes = new byte[bufferSize];

                                while ((bytesRead = sourceData.Read(bytes, 0, bufferSize)) > 0)
                                {
                                    destinationData.Write(bytes, 0, bytesRead);
                                }
                            }
                        }
                    }
                }
            }

            // Copy each child folder using recursion.
            foreach (var subFolderInfo in sourceInfo.GetDirectories())
            {
                var nextFolderInfo = targetInfo.CreateSubdirectory(subFolderInfo.Name);

                CopyFolder(subFolderInfo.FullName, nextFolderInfo.FullName, overrideExisting);
            }
        }

        /// <summary>
        /// Copies the supplied Files to the destination folder 
        /// </summary>
        /// <param name="sourceFiles"></param>
        /// <param name="destinationFolder"></param>
        public static void CopyFiles(List<FileInfo> sourceFiles, string destinationFolder)
        {
            var targetFolderInfo = new DirectoryInfo(destinationFolder);

            if (!targetFolderInfo.Exists)
            {
                Directory.CreateDirectory(destinationFolder);
            }

            foreach (var file in sourceFiles)
            {
                File.Copy(file.FullName, Path.Combine(destinationFolder, file.Name), true);
            }
        }

        public static bool DeleteFolder(string path, bool includeFiles)
        {
            if (FolderExists(path))
            {
                Directory.Delete(path, includeFiles);
                
                var meta = path + ".meta";
                
                if(File.Exists(meta)) File.Delete(meta);

                return true;
            }
            Debug.LogWarning(path + " not found, unable to Delete");

            return false;
        }

        public static bool RenameFolder(string path, string newName)
        {
            if (FolderExists(path))
            {
                var newPath = GetCombinedPath(path.Substring(path.LastIndexOf(GetFolderFromPath(path))), newName);

                Directory.Move(path, newPath);
                
                var meta = path + ".meta";
                
                if(File.Exists(meta)) File.Delete(meta);

                return true;
            }
            Debug.LogWarning(path + " not found, unable to Rename as " + path);

            return false;
        }

        /// <summary>
        /// Iterates all child folders of the parent, and returns the most recently created folder who's name contains the supplied childName
        /// </summary>
        /// <param name="parentFolderPath"></param>
        /// <param name="childName"></param>
        /// <returns></returns>
        public static string GetLatestFolderMatching(string parentFolderPath, string childName)
        {
            parentFolderPath = GetPlatformSpecificPath(parentFolderPath);

            if (FolderExists(parentFolderPath))
            {
                var directories = new DirectoryInfo(parentFolderPath).GetDirectories("*", SearchOption.AllDirectories);

                directories = directories.OrderByDescending(i => i.CreationTime).ToArray();

                foreach (var childFolder in directories)
                {
                    if (childFolder.Name.Contains(childName)) return childFolder.FullName;
                }
            }
            else
            {
                Debug.LogWarning(parentFolderPath + " does not exist");
            }

            return "";
        }

        public static List<FileInfo> GetFilesInFolder(string folderPath, bool includeSubFolders = true, string searchPattern = "*.*")
        {
            folderPath = GetPlatformSpecificPath(folderPath);

            var directoryInfo = new DirectoryInfo(folderPath);

            return directoryInfo.GetFiles(searchPattern, includeSubFolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
        }

        /// <summary>
        /// Returns true if the contents of both folders are identical
        /// </summary>
        /// <param name="folder1Path"></param>
        /// <param name="folder2Path"></param>
        /// <returns></returns>
        public static bool FolderContentsMatch(string folder1Path, string folder2Path)
        {
            var folder1Contents = GetFilesInFolder(folder1Path);
            var folder2Contents = GetFilesInFolder(folder2Path);

            return folder1Contents.SequenceEqual(folder2Contents, new FileCompare());
        }

        /// <summary>
        /// Returns a List of <see cref="FileInfo"/> for all Files found in both folders
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="comparePath"></param>
        /// <returns></returns>
        public static List<FileInfo> GetMatchingFiles(string sourcePath, string comparePath)
        {
            IEnumerable<FileInfo> folder1Files = GetFilesInFolder(sourcePath);
            IEnumerable<FileInfo> folder2Files = GetFilesInFolder(comparePath);

            return folder1Files.Intersect(folder2Files, new FileCompare()).ToList();
        }

        /// <summary>
        /// Returns a List of <see cref="FileInfo"/> for any Files found in the source folder, but not in the compare folder
        /// </summary>
        /// <param name="sourcePath"></param>
        /// <param name="comparePath"></param>
        /// <returns></returns>
        public static List<FileInfo> GetNonMatchingFiles(string sourcePath, string comparePath)
        {
            IEnumerable<FileInfo> folder1Files = GetFilesInFolder(sourcePath);
            IEnumerable<FileInfo> folder2Files = GetFilesInFolder(comparePath);

            return folder1Files.Except(folder2Files, new FileCompare()).ToList();
        }
        
        public static void UnzipFolder(string sourceZipPath, string targetFolderPath)
        {
            if (!FileExists(sourceZipPath))
            {
                Debug.LogError("unable to unzip, invalid path " + sourceZipPath);

                return;
            }
            
            // Ensure the target directory exists
            if (!Directory.Exists(targetFolderPath))
            {
                Directory.CreateDirectory(targetFolderPath);
            }

            // Extract the contents of the zip file to the target directory
            ZipFile.ExtractToDirectory(sourceZipPath, targetFolderPath);
        }

        public static void OpenFolder(string folderPath)
        {
            if(!FolderExists(folderPath)) throw new FileNotFoundException("Unable to open folder " + folderPath);

            Process process = new Process();

            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Windows: Use explorer.exe to open the folder
                process.StartInfo.FileName = "explorer.exe";
                process.StartInfo.Arguments = "\"" + folderPath + "\""; // Quotes for spaces in the path
                process.StartInfo.UseShellExecute = true;
            }
            else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor)
            {
                // macOS: Use the open command to open the folder in Finder
                process.StartInfo.FileName = "open";
                process.StartInfo.Arguments = "\"" + folderPath + "\""; // Quotes for spaces in the path
                process.StartInfo.UseShellExecute = true;
            }

            try
            {
                // Start the process to open the folder
                process.Start();
            }
            catch (System.Exception ex)
            {
                Debug.LogError("Failed to open folder: " + ex.Message);
            }
            
        }


        #endregion

        #region Paths ..................................................................................................

        /// <summary>
        /// Combines and returns supplied paths with the correct path separator based on Windows or other platforms
        /// </summary>
        /// <param name="path1"></param>
        /// <param name="path2"></param>
        /// <param name="path3"></param>
        /// <returns></returns>
        public static string GetCombinedPath(string path1, string path2, string path3 = "")
        {
            return GetPlatformSpecificPath(Path.Combine(path1, path2, path3));
        }
        /// <summary>
        /// Takes an absolute path and returns a path relative to the project folder (useful for AssetDatabase operations)
        /// </summary>
        /// <param name="absolutePath"></param>
        /// <returns></returns>
        public static string GetAssetPath(string absolutePath)
        {
            absolutePath = GetPlatformSpecificPath(absolutePath);

            var assetPath = GetPlatformSpecificPath(StringUtil.TrimString(absolutePath, GetPlatformSpecificPath(Application.dataPath), ""));

            assetPath = assetPath.TrimStart("/");
            assetPath = assetPath.TrimStart("\\");

            assetPath = "Assets/" + assetPath;

            return GetPlatformSpecificPath(assetPath);
        }

        /// <summary>
        /// Removes file extension and stripes path down to just the child folder, as required by <see cref="Resources.Load(string)"/> method.
        /// E.g. "Assets/Resources/Images/Image1.png" returns "Images/Image1";
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetResourceLoadPath(string path)
        {
            path = GetPlatformSpecificPath(path);

            // strip extension from path
            if (path.Contains("."))
            {
                path = path.Substring(0, path.LastIndexOf('.') + 1);
            }

            var loadPath = StringUtil.TrimString(path, GetPlatformSpecificPath(Application.dataPath), "");
            var trimIndex = loadPath.LastIndexOf("Resources", StringComparison.Ordinal);

            return trimIndex < 0 ? "" : loadPath.Substring(trimIndex + 10);
        }

        /// <summary>
        /// Returns supplied path with the correct path separator based on Windows or other platforms
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetPlatformSpecificPath(string path)
        {
            if (path == null)
            {
                return path;
            }
            return ((Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) ?
                path.Replace(@"/", @"\") :
                path.Replace(@"\", @"/"));
        }


        /// <summary>
        /// returns streaming assets path for the current platform
        /// </summary>
        /// <returns></returns>
        public static string GetPlatformStreamingAssetsPath()
        {
            string path = Application.streamingAssetsPath;

            if (!Application.isEditor)
            {
                if (Application.platform == RuntimePlatform.Android)
                {
                    path = "file://" + Application.dataPath + "/raw/";
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {
                    path = "jar:file://" + Application.dataPath + "!/assets/StreamingAssets/";
                }
                else if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
                {
                    path = "file://" + Application.dataPath + "/StreamingAssets/";
                }
                else if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer)
                {
                    path = "file://" + Application.dataPath + "/Resources/StreamingAssets/";
                }
            }

            return path;
        }

        /// <summary>
        /// Combine path with Unity StreamingAssets directory and Convert all directory separators.
        /// </summary>
        /// <param name="path">The path to combine with the StreamingAssets directory.</param>
        /// <returns>The combined path with the StreamingAssets directory and the correct path separator based on the platform.</returns>
        public static string GetPlatformStreamingAssetsPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError("Invalid path supplied to GetPlatformStreamingAssetsPath: Path is null or empty.");
                return "";
            }

            // Normalize all path slashes to forward slashes
            path = path.Replace('\\', '/').TrimEnd('/');

            // Check if path adjustments are needed based on specific prefixes
            string platformSpecificPath = path;
            if (path.StartsWith("file://"))
            {
                platformSpecificPath = path.Substring(7);  // remove "file://" prefix
            }
            else if (path.StartsWith("Assets/StreamingAssets/"))
            {
                platformSpecificPath = path.Substring(24);  // adjust for incorrect start assumption
            }
            else if (path.StartsWith("/"))
            {
                platformSpecificPath = path.Substring(1);  // remove leading slash if present
            }

            // Retrieve platform-specific base path for streaming assets
            string basePlatformPath = Application.streamingAssetsPath;

            // Ensure that all paths use forward slashes and concatenate correctly
            string completePath = $"{basePlatformPath}/{platformSpecificPath}".Replace("//", "/");

            return completePath;
        }
        
        private static char GetApplicationSpecificPathSeparator()
        {
            if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor)
            {
                return '\\';
            }
            else
            {
                return '/';
            }
        }

        /// <summary>
        /// Combine all segments and Convert all directory separators.
        /// </summary>
        public static string Combine(params string[] pathSegments)
        {
            string fullPath = Path.Combine(pathSegments);
            return GetPlatformSpecificPath(fullPath);
        }


        /// <summary>
        /// Combine path with Unity StreamingAssets directory and Convert all directory separators.
        /// </summary>
        public static string CombineWithStreamingAssets(string path)
        {
            return Combine(Application.streamingAssetsPath, path);
        }

        /// <summary>
        /// Combine path with Unity DataPath directory and Convert all directory separators.
        /// </summary>
        public static string CombineWithDataPath(string path)
        {
            return Combine(Application.dataPath, path);
        }
        
        /// <summary>
        /// Combine path with Unity PersistentDataPath directory and Convert all directory separators.
        /// </summary>
        public static string CombineWithPersistentDataPath(string path)
        {
            return Combine(Application.persistentDataPath, path);
        }
        

        /// <summary>
        /// Combine path with Unity TemporaryCachePath directory and Convert all directory separators.
        /// </summary>
        public static string CombineWithTemporaryCachePath(string path)
        {
            return Combine(Application.temporaryCachePath, path);
        }

        #endregion

        #region Helpers ................................................................................................

        private static byte[] GetByteArrayFromStream(FileStream stream)
        {
            var length = Convert.ToInt32(stream.Length);

            var data = new byte[length];

            stream.Read(data, 0, length);
            stream.Close();

            return data;
        }
        
        

        #endregion

    }

    internal class FileCompare : IEqualityComparer<FileInfo>
    {
        public bool Equals(FileInfo f1, FileInfo f2)
        {
            return f1.Name == f2.Name && f1.Length == f2.Length;
        }

        // Return a hash that reflects the comparison criteria. According to the
        // rules for IEqualityComparer<T>, if Equals is true, then the hash codes must  
        // also be equal. Because equality as defined here is a simple value equality, not  
        // reference identity, it is possible that two or more objects will produce the same  
        // hash code.  
        public int GetHashCode(FileInfo fi)
        {
            var s = $"{fi.Name}{fi.Length}";

            return s.GetHashCode();
        }
    }
}
