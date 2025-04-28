using System.Collections.Generic;
using System.Linq;
using EmberAI.Core.Util;
using UnityEngine;

namespace EmberAI.Core
{
    public static class CSExtensions
    {
        #region String .................................................................................................

        /// <summary>
        /// Returns a string with the supplied trimChars removed from the beginning of the string, if found
        /// </summary>
        /// <param name="target"></param>
        /// <param name="trimChars"></param>
        /// <returns></returns>
        public static string TrimStart(this string target, string trimChars)
        {
            return target.TrimStart(trimChars.ToCharArray());
        }

        /// <summary>
        /// Returns a string with the supplied trimChars removed from the end of the string, if found
        /// </summary>
        /// <param name="target"></param>
        /// <param name="trimChars"></param>
        /// <returns></returns>
        public static string TrimEnd(this string target, string trimChars)
        {
            return target.Substring(0, target.Length - trimChars.Length);
        }

        public static bool IsEmptyString(this string target)
        {
            return string.IsNullOrEmpty(target);
        }

        #endregion

        #region Lists ..................................................................................................

        /// <summary>
        /// Adds an Enumerable Collection of type <see cref="T"/> to an existing List of the same type.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void Add<T>(this List<T> target, IEnumerable<T> source)
        {
            target.AddRange(source);
        }

        /// <summary>
        /// Adds an item to the supplied list only if its not already contained
        /// </summary>
        /// <param name="target"></param>
        /// <param name="item"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>True if added, false if already exists</returns>
        public static bool AddIfNotFound<T>(this List<T> target, T item)
        {
            if (item == null)
            {
                Debug.LogError("cannot add null item to List!");
            }

            // create target list if its null.
            if (target == null) target = new List<T>();

            if (target.Contains(item))
            {
                return false;
            }

            target.Add(item);

            return true;
        }

        /// <summary>
        /// Adds items to the supplied list, only if they are not already contained
        /// </summary>
        /// <param name="target"></param>
        /// <param name="items"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static void AddIfNotFound<T>(this List<T> target, params T[] items)
        {
            foreach (var item in items)
            {
                AddIfNotFound(target, item);
            }
        }

        public static void RemoveIfFound<T>(this List<T> target, T item)
        {
            if (target.Contains(item))
            {
                target.Remove(item);
            }
        }

        /// <summary>
        /// Returns a copy of the supplied List
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static List<T> GetCopy<T>(this List<T> source)
        {
            var sourceCopy = new T[source.Count];

            source.CopyTo(sourceCopy);

            return sourceCopy.ToList();
        }

        /// <summary>
        /// Returns a random <see cref="T"/> from the supplied <see cref="List{T}"/>
        /// </summary>
        /// <param name="source"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetRandom<T>(this List<T> source)
        {
            if (source.Count == 0) return default;

            return source[MathUtil.GetRandomNumber(0, source.Count - 1)];
        }

        /// <summary>
        /// Returns a random <see cref="T"/> from the supplied <see cref="List{T}"/>, excluding the defined items
        /// </summary>
        /// <param name="source"></param>
        /// <param name="excludes"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T GetRandom<T>(this List<T> source, params T[] excludes)
        {
            var remainingItems = new List<T>();

            foreach (var item in source)
            {
                if (!excludes.Contains(item)) remainingItems.Add(item);
            }

            return GetRandom(remainingItems);
        }

        /// <summary>
        /// Returns true when supplied index exists within the the supplied <see cref="List{T}"/>
        /// </summary>
        /// <param name="source"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static bool InBounds<T>(this List<T> source, int index)
        {
            return index >= 0 && index < source.Count;
        }

        #endregion

        #region Dictonary Functions.....................................................................................

        public static void AddIfKeyNotFound<T1, T2>(this Dictionary<T1, T2> dict, T1 Key, T2 Value)
        {
            if (!dict.ContainsKey(Key))
            {
                dict.Add(Key, Value);
            }
        }

        #endregion
    }
}