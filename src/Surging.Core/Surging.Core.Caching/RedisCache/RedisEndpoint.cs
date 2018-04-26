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
    /// <remarks>
    /// 	<para>创建：范亮</para>
    /// 	<para>日期：2016/4/2</para>
    /// </remarks>
    public class RedisEndpoint : CacheEndpoint
    {
        /// <summary>
        /// 主机
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public new string Host
        {
            get; set;
        }

        /// <summary>
        /// 端口
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public new int Port
        {
            get; set;
        }

        /// <summary>
        /// 密码
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public string Password
        {
            get; set;
        }

        /// <summary>
        /// 数据库
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public int DbIndex
        {
            get; set;
        }

        public int MaxSize
        {
            get; set;
        }

        public int MinSize
        {
            get;
            set;
        }


        public override string ToString()
        {
            return string.Concat(new string[] { Host, ":", Port.ToString(),"::" ,DbIndex.ToString()});
        }
 
    }
}
