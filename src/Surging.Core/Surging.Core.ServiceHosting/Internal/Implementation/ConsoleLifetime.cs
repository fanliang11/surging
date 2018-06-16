using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Surging.Core.ServiceHosting.Internal.Implementation
{
    public class ConsoleLifetime: IHostLifetime
    {
        private readonly ManualResetEvent _shutdownBlock = new ManualResetEvent(false);
        public ConsoleLifetime(IApplicationLifetime applicationLifetime)
        { 
            ApplicationLifetime = applicationLifetime ?? throw new ArgumentNullException(nameof(applicationLifetime));
        }

        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            ApplicationLifetime.ApplicationStarted.Register(() =>
            {
                Console.WriteLine("服务已启动。 按下Ctrl + C关闭。"); 
            });

            AppDomain.CurrentDomain.ProcessExit += (sender, eventArgs) =>
            {
                ApplicationLifetime.StopApplication();
                _shutdownBlock.WaitOne();
            };
            Console.CancelKeyPress += (sender, e) =>
            {
                e.Cancel = true;
                _shutdownBlock.Set();
                ApplicationLifetime.StopApplication();
            };
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _shutdownBlock.Set();
        }
        private IApplicationLifetime ApplicationLifetime { get; }
    }
}
