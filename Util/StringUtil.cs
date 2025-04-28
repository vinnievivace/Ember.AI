using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;
using Random = System.Random;

namespace EmberAI.Core.Util
{
    public static class StringUtil
    {
        private static Random random = new Random();
        
        private const string emailRegex = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";
        
        /// <summary>
        /// Returns the supplied Objects properties as a string formatted {propertyName : value }
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToString(object obj)
        {
            if (obj == null) return "";
            
            var output = "[" + obj.GetType().Name + "] - ";
            var properties = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            var firstProp = true;

            // Best method: uses the properties discovered using reflection
            if (properties.Length > 0)
            {
                foreach (var prop in properties)
                {
                    if (!firstProp)
                    {
                        output += ", ";
                    }

                    output += prop.Name + ": " + prop.GetValue(obj);

                    firstProp = false;
                }
            }

            // fallback solution: some objects cannot discover properties by reflection
            else
            {
                try
                {
                    var objTypes = ((IEnumerable)obj).Cast<object>().Select(x => x.GetType().ToString()).ToArray();
                    var objValues = ((IEnumerable)obj).Cast<object>().Select(x => x.ToString()).ToArray();

                    for (var i =0; i < objTypes.Length; i++)
                    {
                        if (!firstProp)
                        {
                            output += ", ";
                        }

                        output += objTypes[i]+ ": " + objValues[i];

                        firstProp = false;
                    }
                }
                catch
                {
                    // and some types just dont wanna, like Streams for example
                    output = "Unable to use reflection or object casting on this object!";
                }
                
            }

            return output;
        }

        
        /// <summary>
        /// Returns the supplied set of parameters as a string formatted { propertyName : value } for each item
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ToString(params object[] parameters)
        {
            var output = "";

            foreach (var param in parameters)
            {
                if (output.Length > 0) output += ", ";
                
                output += ToString(param);
            }

            return output;
        }

        /// <summary>
        /// Returns a concatenated string representation of the supplied <see cref="List{T}"/>
        /// </summary>
        /// <param name="items"></param>
        /// <param name="separator"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string ToString<T>(List<T> items, string separator = "|", string prefix = "", string suffix = "")
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(prefix);
            List<string> itemStrings = items.ConvertAll(item => item.ToString());
            stringBuilder.Append(string.Join(separator, itemStrings));
            stringBuilder.Append(suffix);
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Checks if a character is contained within set of AlphaNumeric characters with spaces
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static bool IsAlphaNumericWithSpaces(char input)
        {
            string validChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 ";
            if (!validChars.Contains(input))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns the supplied Dictionary as a string formatted {item.Key : value }, for each item in the Dictionary
        /// </summary>
        /// <param name="dictionary"></param>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <returns></returns>
        public static string ToString<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            var output = "[Dictionary] - ";
            var firstProp = true;

            foreach (var item in dictionary)
            {
                if (!firstProp)
                {
                    output += ", ";
                }

                output += "[ " + item.Key + ": " + item.Value + " ]";

                firstProp = false;
            }

            return output;
        }

        /// <summary>
        /// Returns a randomString of the supplied length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

            return new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public static void CopyStringToClipboard(string text)
        {
            TextEditor te = new TextEditor();
            te.text = text;
                
            te.OnFocus();
            te.Copy();
        }
        
        /// <summary>
        /// Trim a string, returning the value between (not including) the prefix (first index of) and suffix (last index of), useful for File paths etc
        /// </summary>
        /// <param name="value"></param>
        /// <param name="prefix"></param>
        /// <param name="suffix"></param>
        /// <returns></returns>
        public static string TrimString(string value, string prefix, string suffix)
        {
            // Find prefix and suffix indices
            var prefixIndex = string.IsNullOrEmpty(prefix) ? 0 : value.IndexOf(prefix);
            var suffixIndex = string.IsNullOrEmpty(suffix) ? value.Length : value.LastIndexOf(suffix);

            // Check if prefix is found
            if (prefixIndex != -1)
            {
                prefixIndex += prefix.Length;
            }

            // Return the original value if prefix or suffix is not found
            if ((prefixIndex == -1 && !string.IsNullOrEmpty(prefix)) || (suffixIndex == -1 && !string.IsNullOrEmpty(suffix)))
            {
                return value;
            }

            // Handle cases where prefix is not found
            if (prefixIndex == -1 || string.IsNullOrEmpty(prefix))
            {
                prefixIndex = 0;
            }

            // Handle cases where suffix is not found
            if (suffixIndex == -1 || string.IsNullOrEmpty(suffix))
            {
                suffixIndex = value.Length;
            }

            // Return the trimmed string
            return value.Substring(prefixIndex, suffixIndex - prefixIndex);
        }

        
        
        /// <summary>
        /// Uses the Levenshtein Distance formula to determine difference between strings. Useful when trying to match similar strings
        /// E.G Find a GameObject with a name similar to "Left_Upper_Leg" could find matches like "L_UpLeg" etc.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static int GetStringDifference(string a, string b)
        {
            var lenA = a.Length;
            var lenB = b.Length;
            var matrix = new int[lenA + 1, lenB + 1];

            for (var i = 0; i <= lenA; i++)
                matrix[i, 0] = i;

            for (var j = 0; j <= lenB; j++)
                matrix[0, j] = j;

            for (var i = 1; i <= lenA; i++)
            {
                for (var j = 1; j <= lenB; j++)
                {
                    var cost = (b[j - 1] == a[i - 1]) ? 0 : 1;

                    matrix[i, j] = Mathf.Min(Mathf.Min(
                            matrix[i - 1, j] + 1,
                            matrix[i, j - 1] + 1),
                        matrix[i - 1, j - 1] + cost);
                }
            }

            return matrix[lenA, lenB];
        }
    }    
}

