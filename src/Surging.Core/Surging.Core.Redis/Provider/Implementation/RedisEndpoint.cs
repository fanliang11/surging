using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.Redis.Provider.Implementation
{
   public class RedisEndpoint
    {
        /// <summary>
        /// 主机
        /// </summary>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public  string Host
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
        public  int Port
        {
            get; set;
        }

        public string KeySuffix
        {
            get;set;
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
        
        public override string ToString()
        {
            return string.Concat(new string[] { Host, ":", Port.ToString(), "::", DbIndex.ToString() });
        }

    }
}
