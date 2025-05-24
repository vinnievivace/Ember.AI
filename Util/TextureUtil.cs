using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace EmberAI.Core.Util
{
    public static class TextureUtil
    {
        /// <summary>
        /// Loads a Texture2D from a file.
        /// </summary>
        /// <param name="texturePath">The path to the texture file.</param>
        /// <returns>The loaded Texture2D.</returns>
        public static Texture2D Load(string texturePath)
        {
            if (!FileUtil.FileExists(texturePath))
            {
                Debug.LogError(texturePath + " - invalid path, cannot load texture");

                return null;
            }


            byte[] fileData = FileUtil.OpenFileAsByteArray(texturePath);

            try
            {
                Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                texture.LoadImage(fileData);

                return texture;
            }
            catch (Exception e)
            {
                Debug.LogError((e + ": " + texturePath));
                
                return null;
            }
        }
        
        public static async Task<Texture2D> LoadFromURL(string url)
        {
            using UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
            UnityWebRequestAsyncOperation operation = webRequest.SendWebRequest();
            
            while (!operation.isDone) await Task.Yield();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[TextureLoader] Web request failed: {webRequest.error} @ {url}");
                return null;
            }

            return DownloadHandlerTexture.GetContent(webRequest);
        }
        
        public static Texture2D CreateFromImage(string imagePath)
        {
            // Check if the file exists at the given path
            if (!File.Exists(imagePath))
            {
                Debug.LogError(imagePath + " - invalid path, cannot load texture");
                return null;
            }

            try
            {
                // Read the image file as a byte array
                byte[] fileData = File.ReadAllBytes(imagePath);
                
                // Check if fileData is valid
                if (fileData == null || fileData.Length == 0)
                {
                    Debug.LogError("Image file data is invalid or empty.");
                    return null;
                }

                // Create a new Texture2D object with no specified texture format
                Texture2D texture = new Texture2D(2, 2);

                // Load the image data into the texture
                if (texture.LoadImage(fileData)) // Automatically resizes the texture based on the image
                {
                    return texture;
                }
                else
                {
                    Debug.LogError("Failed to load image data into the texture. " + imagePath + ", size = " + fileData.Length);
                    return null;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception occurred while loading texture: {e.Message}");
                return null;
            }
        }
        
        public static bool CanCreateFromImage(string imagePath)
        {
            if (!File.Exists(imagePath)) return false;
            
            byte[] fileData = File.ReadAllBytes(imagePath);
            
            if (fileData.Length == 0) return false;

            Texture2D texture = new Texture2D(2, 2);

            return texture.LoadImage(fileData);

        }
        
        public static Color GetPredominantColor(Texture2D texture)
        {
            Color[] pixels = texture.GetPixels();
            float r = 0, g = 0, b = 0;
            int validPixelCount = 0;

            // Adjust white threshold here (lower to be less strict)
            float whiteThreshold = 0.95f;

            // Iterate through each pixel
            for (int i = 0; i < pixels.Length; i++)
            {
                Color pixel = pixels[i];

                // Skip fully transparent pixels or almost transparent
                if (pixel.a < 0.05f)
                    continue;

                // Skip nearly white pixels based on a less strict threshold
                if (pixel.r > whiteThreshold && pixel.g > whiteThreshold && pixel.b > whiteThreshold)
                    continue;

                // Accumulate the RGB values of the valid pixels
                r += pixel.r;
                g += pixel.g;
                b += pixel.b;
                validPixelCount++;
            }
            
            Debug.unityLogger.Log("Predominant color found: " + validPixelCount + " / " + pixels.Length);

            // If no valid pixels are found, return a default color
            if (validPixelCount == 0)
                return Color.clear;

            // Calculate the average color
            r /= validPixelCount;
            g /= validPixelCount;
            b /= validPixelCount;

            return new Color(r, g, b);
        }
        
        public static Color GetSecondPredominantColor(Texture2D texture)
        {
            Color[] pixels = texture.GetPixels();
            Dictionary<Color, int> colorCount = new Dictionary<Color, int>();
            float whiteThreshold = 0.95f;

            // Iterate through each pixel and populate the color frequency dictionary
            foreach (Color pixel in pixels)
            {
                // Skip fully transparent pixels
                if (pixel.a < 0.05f)
                    continue;

                // Normalize the color (optional), so that near identical colors are treated as the same color
                Color roundedColor = new Color(Mathf.Round(pixel.r * 10) / 10, Mathf.Round(pixel.g * 10) / 10, Mathf.Round(pixel.b * 10) / 10, 1f);

                // Skip near-white colors (or dominant background colors)
                if (roundedColor.r > whiteThreshold && roundedColor.g > whiteThreshold && roundedColor.b > whiteThreshold)
                    continue;

                // Count the occurrence of the color
                if (colorCount.ContainsKey(roundedColor))
                {
                    colorCount[roundedColor]++;
                }
                else
                {
                    colorCount[roundedColor] = 1;
                }
            }

            // If no valid colors found, return a default color (clear)
            if (colorCount.Count == 0)
            {
                Debug.Log("No valid colors found.");
                return Color.clear;
            }

            // Sort colors by their frequency
            List<KeyValuePair<Color, int>> sortedColors = new List<KeyValuePair<Color, int>>(colorCount);
            sortedColors.Sort((pair1, pair2) => pair2.Value.CompareTo(pair1.Value)); // Sort descending by count

            // Return the second predominant color, if available
            if (sortedColors.Count > 1)
            {
                Debug.Log("Second predominant color found.");
                return sortedColors[1].Key; // Second most frequent color
            }

            // If there's no second predominant color, return the first one (only one color found)
            return sortedColors[0].Key;
        }

        public static Cubemap CreateCubemapFromTexture(Texture2D sourceTexture, int faceSize)
        {
            Cubemap cubemap = new Cubemap(faceSize, sourceTexture.format, false);

            /*for (int i = 0; i < 3; i++)
            {
                Color[] facePixels = sourceTexture.GetPixels(i * faceSize, 0, faceSize, faceSize);
                cubemap.SetPixels(facePixels, (CubemapFace)i);
            }*/
            //6912x3456
            //Color[] facePixels = sourceTexture.GetPixels(0, 0, 1024, 1024);
            //cubemap.SetPixels(facePixels, CubemapFace.NegativeX);

            int faceXPos = 0;
            int faceYPos = 0;
            
            Debug.Log(sourceTexture + " = " + sourceTexture.width + " x " + sourceTexture.height);
            
            foreach (CubemapFace face in Enum.GetValues(typeof(CubemapFace)))
            {
                Debug.Log(face + " " + faceXPos + ", " + faceYPos);
                
                if (face != CubemapFace.Unknown)
                {
                    Color[] pixels = sourceTexture.GetPixels(faceSize * faceXPos,faceSize * faceYPos,faceSize, faceSize);
                    
                    cubemap.SetPixels(pixels, face);

                    if (faceXPos == 2)
                    {
                        faceXPos = 0;
                        faceYPos++;
                    }
                    else
                    {
                        faceXPos++;    
                    }
                    
                }
            }
            
            cubemap.Apply();
            return cubemap;
        }
        
        public static void DebugCubeMap(Cubemap cubemap)
        {
            if (cubemap == null)
            {
                Debug.LogError("DebugCubeMap: The cubemap provided is null.");
                return;
            }

            // Output basic information about the Cubemap
            Debug.Log($"Cubemap Information: \n" +
                      $"Face Size: {cubemap.width}x{cubemap.height} pixels\n" +
                      $"Texture Format: {cubemap.format}\n" +
                      $"Mipmap Count: {cubemap.mipmapCount}");

            // Optionally, inspect pixel data from each face
            foreach (CubemapFace face in System.Enum.GetValues(typeof(CubemapFace)))
            {
                if (face != CubemapFace.Unknown)
                {
                    Color pixel = cubemap.GetPixel(face, cubemap.width / 2, cubemap.height / 2);
                    Debug.Log($"Face {face}: Pixel color at center is {pixel}");
                }
            }
        }
        
        /// <summary>
        /// Returns a sub section of the supplied <see cref="Texture2D"/>. Note the source Texture must be set to read/write in its import settings.
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public static Texture2D GetRegion(Texture2D texture, int x, int y, int width, int height)
        {
            if (!texture.isReadable)
            {
                Debug.LogError(nameof(GetRegion) + " requires the Texture to be Readable, but it is not");

                return texture;
            }
            
            Texture2D result = new Texture2D(width, height, TextureFormat.RGBA32, false);

            // We need to modify the y value since Unity textures have their origin at the bottom-left
            y = texture.height - y - height;

            Color[] pixels = texture.GetPixels(x, y, width, height);
            result.SetPixels(pixels);
            result.Apply();

            return result;
        }
    }
}