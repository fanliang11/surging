using Surging.Core.CPlatform.Network;
using Surging.Core.Protocol.Tcp.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.RuleParser.Implementation
{
    public class ScriptPayloadParserBuilder : IRulePayloadParserBuilder
    {
        public PayloadParserType ParserType { get; } = PayloadParserType.Script;

        public Action<RulePipePayloadParser> Build(ValueObject config)
        {
            String script = config.GetVariableValue("script", "");
            return parser => { };
        }
    }
} 