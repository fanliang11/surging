using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    /// <summary>
    /// 服务端用来阻止Host主线程退出，直到按下Ctrl+C
    /// </summary>
    public class ConsoleLifetime : IHostLifetime
    {
        #region 字段

        /// <summary>
        /// Defines the _shutdownBlock
        /// </summary>
        private readonly ManualResetEvent _shutdownBlock = new ManualResetEvent(false);

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsoleLifetime"/> class.
        /// </summary>
        /// <param name="applicationLifetime">The applicationLifetime<see cref="IApplicationLifetime"/></param>
        public ConsoleLifetime(IApplicationLifetime applicationLifetime)
        {
            ApplicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the ApplicationLifetime
        /// </summary>
        private IApplicationLifetime ApplicationLifetime { get; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Dispose
        /// </summary>
        public void Dispose()
        {
            _shutdownBlock.Set();
        }

        /// <summary>
        /// The StopAsync
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// The WaitForStartAsync
        /// </summary>
        /// <param name="cancellationToken">The cancellationToken<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            ApplicationLifetime.ApplicationStarted.Register(() =>
            {
                Console.WriteLine("服务已启动。 按下Ctrl + C关闭。");
            });

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                ApplicationLifetime.StopApplication();
                //阻止程序主线程自动退出，等待退出信号
                _shutdownBlock.WaitOne();
            };
            //按下Ctrl+C退出程序
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _shutdownBlock.Set();
                ApplicationLifetime.StopApplication();
            };
            return Task.CompletedTask;
        }

        #endregion 方法
    }
}