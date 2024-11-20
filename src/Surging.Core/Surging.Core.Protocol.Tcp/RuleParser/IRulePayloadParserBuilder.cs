using Surging.Core.CPlatform.Network;
using Surging.Core.Protocol.Tcp.RuleParser.Implementation;
using Surging.Core.Protocol.Tcp.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.RuleParser
{
    public interface IRulePayloadParserBuilder
    {
        PayloadParserType ParserType { get;  }

        Action<RulePipePayloadParser> Build(ValueObject config);
    }
}
