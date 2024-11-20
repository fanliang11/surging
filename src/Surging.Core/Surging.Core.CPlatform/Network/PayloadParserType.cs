using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Network
{
    public enum PayloadParserType
    {
        Direct,//不处理
        Delimited,//分隔符
        Script,//自定义脚本 
        FixedLength,//"固定长度"
        LengthField
    }
}
