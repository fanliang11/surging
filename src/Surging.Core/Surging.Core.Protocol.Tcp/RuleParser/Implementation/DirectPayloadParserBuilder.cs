using Surging.Core.CPlatform.Network;
using Surging.Core.Protocol.Tcp.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.RuleParser.Implementation
{
    public class DirectPayloadParserBuilder : IRulePayloadParserBuilder
    {
        public PayloadParserType ParserType { get; } = PayloadParserType.Direct;

        public Action<RulePipePayloadParser> Build(ValueObject config)
        {
            return parser => { };
        }
    }
}
