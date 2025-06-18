using DotNetty.Buffers;
using Surging.Core.CPlatform.Network;
using Surging.Core.Protocol.Tcp.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.RuleParser.Implementation
{
    internal class FixLengthPayloadParserBuilder : IRulePayloadParserBuilder
    { 
        public PayloadParserType ParserType { get; } = PayloadParserType.FixedLength;

        public Action<RulePipePayloadParser> Build(ValueObject config)
        { 
            var size = config.GetVariableValue<int?>("size");

            //包长度
            if (size == null)
                new ArgumentNullException("size can not be null");

            return parser=> parser.Fixed(size.Value);
        }
    }
}
