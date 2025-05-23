using DotNetty.Common.Utilities;
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
    public class DefaultPayloadParserBuilder : IRulePayloadParserBuilder
    {
        private readonly ConcurrentDictionary<PayloadParserType, IRulePayloadParserBuilder> _payloadParserBuilders = new ConcurrentDictionary<PayloadParserType, IRulePayloadParserBuilder>();
        public DefaultPayloadParserBuilder()
        {
            Add(PayloadParserType.LengthField,new LengthFieldPayloadParserBuilder());
        }
        public PayloadParserType ParserType { get; set; }

        public Action<RulePipePayloadParser> Build( ValueObject config)
        {
            IRulePayloadParserBuilder builder;
            Action<RulePipePayloadParser> result = p => { };
           if ( _payloadParserBuilders.TryGetValue(ParserType, out builder))
            {
                result = builder.Build(config);
            }
            else
            {
                throw new NotSupportedException("unsupported parser:" + ParserType);
            }
            return result;
        }

        public void Add(PayloadParserType parserType, IRulePayloadParserBuilder strategy)
        {
            _payloadParserBuilders.TryAdd(parserType, strategy);
        }
    }
}
