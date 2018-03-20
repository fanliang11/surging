using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Core
{
    /// <summary>
    /// 搜索条件排序基类Dto。
    /// </summary>
    public abstract class BaseSortCriteriaDto : BaseDto
    {
        /// <summary>
        /// 创建时间
        /// </summary>
      
        public static readonly string CREATE_TIME = "CreateTime";

        /// <summary>
        /// 修改时间
        /// </summary>
      
        public static readonly string UPDATE_TIME = "UpdateTime";

        /// <summary>
        /// 正向/反向排序
        /// </summary>
      
        public bool Ascending { set; get; }

        /// <summary>
        /// 排序名称
        /// </summary>
      
        public string SortPropertyName { set; get; }
    }
}
