using System.Diagnostics;

namespace Surging.Core.Caching.Utilities
{
    /// <summary>
    /// Defines the <see cref="DebugCheck" />
    /// </summary>
    public sealed class DebugCheck
    {
        #region 方法

        /// <summary>
        /// The NotEmpty
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        [Conditional("DEBUG")]
        public static void NotEmpty(string value)
        {
            Debug.Assert(!string.IsNullOrWhiteSpace(value));
        }

        /// <summary>
        /// The NotNull
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value<see cref="T"/></param>
        [Conditional("DEBUG")]
        public static void NotNull<T>(T value) where T : class
        {
            Debug.Assert(value != null);
        }

        /// <summary>
        /// The NotNull
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value<see cref="T?"/></param>
        [Conditional("DEBUG")]
        public static void NotNull<T>(T? value) where T : struct
        {
            Debug.Assert(value != null);
        }

        #endregion 方法
    }
}