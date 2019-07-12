using Surging.Core.Caching.Interfaces;
using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Surging.Core.Caching.RedisCache
{
    /// <summary>
    /// redis 终端
    /// </summary>
    public class RedisEndpoint : CacheEndpoint
    {
        #region 属性

        /// <summary>
        /// Gets or sets the DbIndex
        /// 数据库
        /// </summary>
        public int DbIndex { get; set; }

        /// <summary>
        /// Gets or sets the Host
        /// 主机
        /// </summary>
        public new string Host { get; set; }

        /// <summary>
        /// Gets or sets the MaxSize
        /// </summary>
        public int MaxSize { get; set; }

        /// <summary>
        /// Gets or sets the MinSize
        /// </summary>
        public int MinSize { get; set; }

        /// <summary>
        /// Gets or sets the Password
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the Port
        /// 端口
        /// </summary>
        public new int Port { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return string.Concat(new string[] { Host, ":", Port.ToString(), "::", DbIndex.ToString() });
        }

        #endregion 方法
    }
}