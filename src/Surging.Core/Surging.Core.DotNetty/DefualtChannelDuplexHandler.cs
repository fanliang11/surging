using DotNetty.Handlers.Timeout;
using DotNetty.Transport.Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DotNetty
{
    public class DefualtChannelDuplexHandler: ChannelDuplexHandler
    {
        public override void UserEventTriggered(IChannelHandlerContext context, object evt)
        {
            if (evt is IdleStateEvent) {
                IdleStateEvent e = (IdleStateEvent)evt;
                if (e.State == IdleState.ReaderIdle)
                {
                    // 连接已读空闲，执行清理操作
                    context.CloseAsync(); // 或者其他清理操作
                }
            }
        }
    }
}
