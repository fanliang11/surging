using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Surging.Core.Protocol.Mqtt.Util
{
    /// <summary>
    /// Defines the <see cref="MessageIdGenerater" />
    /// </summary>
    public class MessageIdGenerater
    {
        #region 字段

        /// <summary>
        /// Defines the _index
        /// </summary>
        private static int _index;

        /// <summary>
        /// Defines the _lock
        /// </summary>
        private static int _lock;

        #endregion 字段

        #region 方法

        /// <summary>
        /// The GenerateId
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
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

        #endregion 方法
    }
}