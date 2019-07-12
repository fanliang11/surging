using System;

namespace Surging.Core.CPlatform.Utilities
{
    /// <summary>
    /// Defines the <see cref="Check" />
    /// </summary>
    public sealed class Check
    {
        #region 方法

        /// <summary>
        /// The CheckCondition
        /// </summary>
        /// <param name="condition">The condition<see cref="Func{bool}"/></param>
        /// <param name="formatErrorText">The formatErrorText<see cref="string"/></param>
        /// <param name="parameters">The parameters<see cref="string[]"/></param>
        public static void CheckCondition(Func<bool> condition, string formatErrorText, params string[] parameters)
        {
            if (condition.Invoke())
            {
                throw new ArgumentException(string.Format(CPlatformResource.ArgumentIsNullOrWhitespace, parameters));
            }
        }

        /// <summary>
        /// The CheckCondition
        /// </summary>
        /// <param name="condition">The condition<see cref="Func{bool}"/></param>
        /// <param name="parameterName">The parameterName<see cref="string"/></param>
        public static void CheckCondition(Func<bool> condition, string parameterName)
        {
            if (condition.Invoke())
            {
                throw new ArgumentException(string.Format(CPlatformResource.ArgumentIsNullOrWhitespace, parameterName));
            }
        }

        /// <summary>
        /// The NotEmpty
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        /// <param name="parameterName">The parameterName<see cref="string"/></param>
        /// <returns>The <see cref="string"/></returns>
        public static string NotEmpty(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException(string.Format(CPlatformResource.ArgumentIsNullOrWhitespace, parameterName));
            }

            return value;
        }

        /// <summary>
        /// The NotNull
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value<see cref="T"/></param>
        /// <param name="parameterName">The parameterName<see cref="string"/></param>
        /// <returns>The <see cref="T"/></returns>
        public static T NotNull<T>(T value, string parameterName) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        /// <summary>
        /// The NotNull
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The value<see cref="T?"/></param>
        /// <param name="parameterName">The parameterName<see cref="string"/></param>
        /// <returns>The <see cref="T?"/></returns>
        public static T? NotNull<T>(T? value, string parameterName) where T : struct
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }

            return value;
        }

        #endregion 方法
    }
}