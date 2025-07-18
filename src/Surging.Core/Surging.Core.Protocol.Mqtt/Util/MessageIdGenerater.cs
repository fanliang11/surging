using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.Protocol.Mqtt.Util
{
   public class MessageIdGenerater
    {

        private static  int _index;
        private static int _lock;
        public static int GenerateId()
        {
            for (; ; )
            {
                if (Interlocked.Exchange(ref _lock, 1) != 0)
                {
                    default(SpinWait).SpinOnce();
                    continue;
                }
                if (int.MaxValue > _index)
                    _index++;
                else
                    _index = 0;
          
                Interlocked.Exchange(ref _lock, 0);
                return _index;
            }
        }
    }
}
