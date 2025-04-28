using System;
using System.Collections.Generic;

namespace EmberAI.Core
{
    public static class ListExtensions
    {
        /// <summary>
        /// Returns true if an item exists at the specified index.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="index"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool IndexIsValid<T>(this List<T> list, int index)
        {
            return index >= 0 && index < list.Count;
        }
        
        
        public static IComparer<T> ThenBy<T>(this IComparer<T> comparer1,
            IComparer<T> comparer2)
        {
            return new ChainedComparer<T>(comparer1, comparer2);
        }

        private class ChainedComparer<T> : IComparer<T>
        {
            private readonly IComparer<T> comparer1;
            private readonly IComparer<T> comparer2;

            public ChainedComparer(IComparer<T> comparer1,
                IComparer<T> comparer2)
            {
                this.comparer1 = comparer1;
                this.comparer2 = comparer2;
            }

            public int Compare(T x, T y)
            {
                int result = this.comparer1.Compare(x, y);
                if (result == 0) result = this.comparer2.Compare(x, y);
                return result;
            }
        }
        
        
        public static void AddUnique<T>(this List<T> list, T element)
        {
            if (!list.Contains(element))
            {
                list.Add(element);
            }
        }
        
        public static void AddRangeUnique<T>(this List<T> list, IEnumerable<T> elements)
        {
            foreach (var element in elements)
            {
                list.AddUnique(element);
            }
        }
        
        public static void AddRangeFilteredAndUnique<T>(this List<T> list, IEnumerable<T> elements, Func<T, bool> filterCallback)
        {
            foreach (var element in elements)
            {
                if (filterCallback.Invoke(element))
                {
                    list.AddUnique(element);
                }
                
            }
        }
        
        public static void Resize<T>(this List<T> list, int size) where T : new()
        {
            var currentSize = list.Count;

            if (size < currentSize)
            {
                list.RemoveRange(size, currentSize - size);
            }
            else if(size > currentSize)
            {
                for (var i = currentSize; i < size; i++)
                {
                    list.Add(new T());
                }
            }
        }
        
        private static Random _randomNumberGenerator = new ();  

        public static void Shuffle<T>(this IList<T> list, Random randomGenerator = null)
        {
            var random = randomGenerator ?? _randomNumberGenerator;
            int n = list.Count;  
            while (n > 1) {  
                n--;
                int k = random.Next(n + 1);  
                T value = list[k];  
                list[k] = list[n];  
                list[n] = value;  
            }  
        }
    }
}