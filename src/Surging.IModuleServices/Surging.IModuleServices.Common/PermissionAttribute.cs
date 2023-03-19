using Surging.Core.CPlatform.Filters.Implementation;
using Surging.Core.CPlatform.Routing;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Transport.Implementation;
using Surging.Core.CPlatform.Utilities;
using Surging.IModuleServices.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;

namespace Surging.IModuleServices.Common
{
    public class PermissionAttribute : BaseActionFilterAttribute
    {
        private readonly ISerializer<string> _serializer;
        public PermissionAttribute()
        {
            _serializer = ServiceLocator.Current.Resolve<ISerializer<string>>();
        }
        public override Task OnActionExecutingAsync(ServiceRouteContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var payload= RpcContext.GetContext().GetAttachment("payload");
            var model= _serializer.Deserialize(payload.ToString().Trim('"').Replace("\\",""),typeof(UserModel));
            //  actionExecutedContext.ResultMessage.ExceptionMessage = "no permission";
            return Task.FromResult(model);
        }
    }
}
