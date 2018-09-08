using Microsoft.AspNetCore.Builder;

namespace Surging.Core.KestrelHttpServer
{
    internal class Startup
    {

        public void Configure(IApplicationBuilder app)
        {
            app.Run(async (context) =>
            { 
            });
        }
    }
}