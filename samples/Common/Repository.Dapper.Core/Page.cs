using System;
using System.Collections.Generic;

namespace Repository.Dapper.Core
{
    public class Page<T> 
    {

        /// <summary>
        /// 总条目数
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// 每页条目数
        /// </summary>
        public long ItemsPerPage { get; set; }

        /// <summary>
        /// 当前页码
        /// </summary>
        public long CurrentPage { get; set; }

        public IEnumerable<T> Items { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public long TotalPages
        {
            get { return (long)Math.Ceiling((decimal)TotalItems / ItemsPerPage); }
        }
    }
}