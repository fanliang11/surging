using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.ServiceHosting
{
    /// <summary>
    /// Defines the <see cref="ServiceCollectionExtensions" />
    /// </summary>
    internal static class ServiceCollectionExtensions
    {
        #region 方法

        /// <summary>
        /// The Clone
        /// </summary>
        /// <param name="serviceCollection">The serviceCollection<see cref="IServiceCollection"/></param>
        /// <returns>The <see cref="IServiceCollection"/></returns>
        public static IServiceCollection Clone(this IServiceCollection serviceCollection)
        {
            IServiceCollection clone = new ServiceCollection();
            foreach (var service in serviceCollection)
            {
                clone.Add(service);
            }
            return clone;
        }

        #endregion 方法
    }
}