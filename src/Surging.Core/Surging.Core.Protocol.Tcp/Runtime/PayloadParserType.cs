namespace Surging.Core.Protocol.Tcp.Runtime
{
    public enum PayloadParserType
    {
        Direct,//不处理
        FixedLength,//"固定长度"
        Delimited,//分隔符
        Script//自定义脚本
    }
}
