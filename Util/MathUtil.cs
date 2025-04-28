using System;
using UnityEngine;
using Random = System.Random;

namespace EmberAI.Core.Util
{
    public static class MathUtil
    {
        private static Random random = new Random();

        /// <summary>
        /// Creates the <see cref="Random"/> instance used by GetRandom methods, using the supplied seed to ensure deterministic results
        /// (i.e. the same sequence of random numbers each time its called)
        /// </summary>
        /// <param name="seed"></param>
        public static void SetRandomSeed(int seed)
        {
            random = new Random(seed);
        }
        
        #region Random Numbers .............................................................................................

        /// <summary>
        /// Returns a random int within the supplied range.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static int GetRandomNumber(int min, int max)  
        {
            // note min is inclusive, max is exclusive, hence + 1
            return random.Next(min, max + 1);  
        }

        /// <summary>
        /// Returns a random float within the supplied range.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static float GetRandomNumber(float min, float max)
        {
            double range = max - min;
            var sample = random.NextDouble();
            var scaled = (sample * range) + min;
            
            return (float)scaled;
              
        }
        
        #endregion
        
        #region Rounding ...................................................................................................

        public static void RoundToDecimal(ref Vector2 source, int decimals)
        {
            source.Set((float)Math.Round(source.x, decimals), (float)Math.Round(source.y, decimals));
        }

        public static void RoundToDecimal(ref float source, int decimals)
        {
            source = (float)Math.Round(source, decimals);
        }
        
        public static void RoundToDecimal(ref double source, int decimals)
        {
            source = (double)Math.Round(source, decimals);
        }

        public static Vector2 RoundToInteger(Vector2 source)
        {
            return new Vector2((float)Math.Round(source.x, 0), (float)Math.Round(source.y, 0));
        }

        public static void RoundToInteger(long source)
        {
            source = Convert.ToInt32(source);
        }

        #endregion

        #region Equality ...................................................................................................

        /// <summary>
        /// Checks if 2 <see cref="Vector2"/> are equal, within the supplied tolerance value
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool CheckEquality(Vector2 vector1, Vector2 vector2, float tolerance = Vector2.kEpsilon)
        {
            return Math.Abs(vector1.x - vector2.x) <= tolerance && Math.Abs(vector1.y - vector2.y) <= tolerance;
        }

        /// <summary>
        /// Checks if 2 <see cref="Vector3"/> are equal, within the supplied tolerance value
        /// </summary>
        /// <param name="vector1"></param>
        /// <param name="vector2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool CheckEquality(Vector3 vector1, Vector3 vector2, float tolerance = Vector3.kEpsilon)
        {
            return Math.Abs(vector1.x - vector2.x) <= tolerance &&
                   Math.Abs(vector1.y - vector2.y) <= tolerance &&
                   Math.Abs(vector1.z - vector2.z) <= tolerance;
        }

        /// <summary>
        /// Checks if 2 <see cref="float"/> are equal, within the supplied tolerance value
        /// </summary>
        /// <param name="float1"></param>
        /// <param name="float2"></param>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        public static bool CheckEquality(float float1, float float2, float tolerance = float.Epsilon)
        {
            return Math.Abs(float1 - float2) <= tolerance;
        }

        #endregion
        
        #region Range ......................................................................................................
        
        /// <summary>
        /// Takes a value within a supplied range and outputs the equivalent value based on a targetRange
        /// </summary>
        /// <param name="value"></param>
        /// <param name="valueRangeMin"></param>
        /// <param name="valueRangeMax"></param>
        /// <param name="targetRangeMin"></param>
        /// <param name="targetRangeMax"></param>
        /// <returns></returns>
        public static float ConvertValueRange(float value, float valueRangeMin, float valueRangeMax, float targetRangeMin, float targetRangeMax)
        {
            return (value - valueRangeMin) / (valueRangeMax - valueRangeMin) * (targetRangeMax - targetRangeMin) + targetRangeMin;
        }
        
        #endregion
        
        #region Vector3 ................................................................................................

        /// <summary>
        /// Returns the supplied <see cref="Vector3"/> with x,y,z properties clamped to the supplied min/max values.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static Vector3 Clamp(Vector3 value, float min, float max)
        {
            value.x = Mathf.Clamp(value.x, min, max);
            value.y = Mathf.Clamp(value.y, min, max);
            value.z = Mathf.Clamp(value.z, min, max);

            return value;
        }
        
        #endregion ................................................................................................
        
        #region Numerics ...............................................................................................
        
        // convert Vector2 / Vector3 and Quaternion between System and Unity classes, useful for JSON etc
        
        public static System.Numerics.Vector2 GetNumericVector2(Vector2 source)
        {
            return new System.Numerics.Vector2(source.x, source.y);
        }
        public static System.Numerics.Vector3 GetNumericVector3(Vector3 source)
        {
            return new System.Numerics.Vector3(source.x, source.y, source.z);
        }

        public static System.Numerics.Quaternion GetNumericQuaternion(Quaternion source)
        {
            return new System.Numerics.Quaternion(source.x, source.y, source.z, source.w);
        }

        public static Vector2 GetVector2FromNumerics(System.Numerics.Vector2 source)
        {
            return new Vector2(source.X, source.Y);
        }

        public static Vector3 GetVector3FromNumerics(System.Numerics.Vector3 source)
        {
            return new Vector3(source.X, source.Y, source.Z);
        }

        public static Quaternion GetQuaternionFromNumerics(System.Numerics.Quaternion source)
        {
            return new Quaternion(source.X, source.Y, source.Z, source.W);
        }

        #endregion
    }    
}