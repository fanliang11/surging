using DotNetty.Buffers;
using Surging.Core.CPlatform.Network;
using Surging.Core.Protocol.Tcp.Runtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.RuleParser.Implementation
{
    public class LengthFieldPayloadParserBuilder : IRulePayloadParserBuilder
    {


        public PayloadParserType ParserType { get; } = PayloadParserType.LengthField;

        public Action<RulePipePayloadParser> Build(ValueObject config)
        {
            //偏移量
            int offset = config.GetVariableValue<int>("offset");

            //包长度
            int len = config.GetVariableValue<int>("length", config.GetVariableValue<int>("to", 4 - offset));

            var le = config.GetVariableValue<bool>("little", false);

            int initLength = offset + len;

            Func<IByteBuffer, int> lengthParser;
            switch (len)
            {
                case 1:
                    lengthParser = buffer => (int)buffer.GetUnsignedInt(offset);
                    break;
                case 2:
                    lengthParser =
                        le ? buffer => buffer.GetUnsignedShortLE(offset)
                            : buffer => buffer.GetUnsignedShort(offset);
                    break;
                case 3:
                    lengthParser =
                        le ? buffer => buffer.GetUnsignedMediumLE(offset)
                            : buffer => (int)buffer.GetUnsignedMedium(offset);
                    break;
                case 4:
                    lengthParser =
                        le ? buffer => buffer.GetIntLE(offset)
                            : buffer => (int)buffer.GetInt(offset);
                    break;
                case 8:
                    lengthParser =
                        le ? buffer => (int)buffer.GetLongLE(offset)
                            : buffer => (int)buffer.GetLong(offset);
                    break;
                default:
                    throw new ArgumentOutOfRangeException("illegal length:" + len);
            }


            return (parser) => parser
                .Fixed(initLength)
            .Handler(buffer =>
            {
                int next = lengthParser.Invoke(buffer);
                parser.Result(buffer)
                      .Fixed(next);
            })
            .Handler(buffer => parser
                .Result(buffer)
                .Complete());
        }
    }
}
