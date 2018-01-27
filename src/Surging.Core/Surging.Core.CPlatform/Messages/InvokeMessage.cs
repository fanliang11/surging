using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Messages
{
   public  class InvokeMessage
    {
        public int Handle { get; set; }
        
        public bool DecodeJOject { get; set; }

        public string ServiceKey { get; set; }

        /// <summary>
        /// 服务参数。
        /// </summary>
        public IDictionary<string, object> Parameters { get; set; }
    }
}
