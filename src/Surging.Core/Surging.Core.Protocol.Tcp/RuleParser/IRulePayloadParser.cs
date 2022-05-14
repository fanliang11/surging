using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.RuleParser
{
    public interface IRulePayloadParser
    { 
      
        void Handle(IByteBuffer buffer);

        void Reset();

        void Build(IByteBuffer buffer);

        void Close();

        ISubject<IByteBuffer> HandlePayload();
    }
}
