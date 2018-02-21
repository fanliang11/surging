using Surging.Core.CPlatform.Cache;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.HashAlgorithms
{
    /// <summary>
    /// 哈希节点对象
    /// </summary>
    /// <remarks>
    /// 	<para>创建：范亮</para>
    /// 	<para>日期：2016/4/2</para>
    /// </remarks>
    public class ConsistentHashNode: CacheEndpoint
    {
        /// <summary>
        /// 缓存目标类型
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public CacheTargetType Type
        {
            get;
            set;
        }

        /// <summary>
        /// 主机
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public new string Host { get; set; }

        /// <summary>
        /// 端口
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public new string Port { get; set; }

        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public string Password { get; set; }

        /// <summary>
        /// 数据库
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public string Db
        {
            get; set;
        }

        private string _maxSize = "50";
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

        private string _minSize = "1";
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

        public override string ToString()
        {
            return string.Concat(new string[] { Host, ":", Port.ToString() });
        }
    }
}
