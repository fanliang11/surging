using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.KestrelHttpServer
{
    public abstract class ActionResult: IActionResult
    {
        public virtual Task ExecuteResultAsync(ActionContext context)
        {
            ExecuteResult(context);
            return Task.CompletedTask;
        }

        
        public virtual void ExecuteResult(ActionContext context)
        {
        }
    }
}
