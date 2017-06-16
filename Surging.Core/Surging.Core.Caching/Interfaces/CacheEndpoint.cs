using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Caching.Interfaces
{
    public abstract class CacheEndpoint
    {
        /// <summary>
        /// 主机
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public string Host
        {
            get;
            set;
        }

        /// <summary>
        /// 端口
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public int Port
        {
            get;
            set;
        }
    }
}
