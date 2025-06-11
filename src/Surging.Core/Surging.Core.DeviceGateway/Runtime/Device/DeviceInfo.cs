using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.device
{
    public class DeviceInfo
    {
        public string Id { get; set; }

        public string ProductId { get; set; }

        /**
         * 产品版本
         */
        public string ProductVersion {  get; set; }

        /**
         * 消息协议
         *
         * @see ProtocolSupport#getId()
         */
        public string Protocol {  get; set; }

        public string Metadata { get; set; }

        /**
         * 其他配置
         */
        public Dictionary<string, object> Configuration = new Dictionary<string, object>();
    }
}
