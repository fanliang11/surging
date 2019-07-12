using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.HashAlgorithms
{
    /// <summary>
    /// 哈希节点对象
    /// </summary>
    public class ConsistentHashNode : CacheEndpoint
    {
        #region 字段

        /// <summary>
        /// Defines the _maxSize
        /// </summary>
        private string _maxSize = "50";

        /// <summary>
        /// Defines the _minSize
        /// </summary>
        private string _minSize = "1";

        #endregion 字段

        #region 属性

        /// <summary>
        /// Gets or sets the Db
        /// 数据库
        /// </summary>
        public string Db { get; set; }

        /// <summary>
        /// Gets or sets the Host
        /// 主机
        /// </summary>
        public new string Host { get; set; }

        /// <summary>
        /// Gets or sets the MaxSize
        /// </summary>
        public string MaxSize
        {
            get
            {
                return _maxSize;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _maxSize = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the MinSize
        /// </summary>
        public string MinSize
        {
            get
            {
                return _minSize;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    _minSize = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the Password
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets or sets the Port
        /// 端口
        /// </summary>
        public new string Port { get; set; }

        /// <summary>
        /// Gets or sets the Type
        /// 缓存目标类型
        /// </summary>
        public CacheTargetType Type { get; set; }

        /// <summary>
        /// Gets or sets the UserName
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public override string ToString()
        {
            return string.Concat(new string[] { Host, ":", Port.ToString() });
        }

        #endregion 方法
    }
}