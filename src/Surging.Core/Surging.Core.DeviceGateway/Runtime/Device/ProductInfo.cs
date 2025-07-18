using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device
{
    public class ProductInfo
    {
        public string Id;

        /**
         * 消息协议
         */
        public string Protocol;

        /**
         * 元数据
         */
        public string Metadata;

        /**
         * 其他配置
         */
        private Dictionary<string, object> Configuration = new Dictionary<string, object>();
    }
}
