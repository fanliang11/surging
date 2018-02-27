using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Core
{
   public class BasePagedRequestDto: BaseRequestDto
    {
        /// <summary>
        /// 页码号，从1开始，可空(返回全部记录)
        /// </summary>
        public int PageIndex { get; set; }

        /// <summary>
        /// 每页显示的记录数，可空(返回全部记录)
        /// </summary>
        public int PageSize { get; set; }
    }
}
