using System;


namespace EmberAI.Core.Util
{
    /// <summary>
    /// Generic Date /Time related utilities
    /// </summary>
    public class DateTimeUtil
    {
        /// <summary>
        /// Returns a <see cref="TimeSpan"/> representing the difference between 2 <see cref="DateTime"/> values.
        /// </summary>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static TimeSpan GetTimeDifference(DateTime startTime, DateTime endTime)
        {
            return (endTime - startTime);
        }
    }
}