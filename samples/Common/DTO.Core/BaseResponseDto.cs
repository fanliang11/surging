using System;
using System.Collections.Generic;
using System.Text;

namespace DTO.Core
{
    public class BaseResponseDto:BaseDto
    {
        /// <summary>
        /// 请求结果,请求成功/失败
        /// </summary>
        public bool OperateFlag { get; set; }

        /// <summary>
        /// 服务端错误信息
        /// </summary>
        public string FlagErrorMsg { get; set; }


    }

   public class BaseTreeResponseDto: BaseResponseDto
    {
        public List<BaseTreeDto> Tree { get; set; }
    }

    public class BaseTreeDto
    {
        public string Id { get; set; }
        public string PId { get; set; }
        public string Name { get; set; }
    }

    public class BaseListResponseDto : BaseResponseDto
    {
        public dynamic Result { get; set; }
    }

}
