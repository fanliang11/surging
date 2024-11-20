using Autofac;
using Microsoft.AspNetCore.Http;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Serialization;
using Surging.Core.CPlatform.Utilities;
using Surging.Core.KestrelHttpServer.Filters;
using Surging.Core.KestrelHttpServer.Filters.Implementation;
using Surging.Core.Protokollwandler.Extensions;
using Surging.Core.Protokollwandler.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Surging.Core.Protokollwandler.Internal.Implementation;
using Surging.Core.Protokollwandler.Metadatas;
using Surging.Core.CPlatform.Runtime.Server;
using Surging.Core.CPlatform.Convertibles;

namespace Surging.Core.Protokollwandler.Filters
{
    public class ActionFilterAttribute : IActionFilter
    {
        private readonly ISerializer<string> _serializer;
        private readonly IEnumerable<IExceptionFilter> _filters;
        private readonly IServiceEntryLocate _serviceEntryLocate;
        private readonly ITypeConvertibleService _typeConvertibleService;
        public ActionFilterAttribute(IEnumerable<IExceptionFilter> filters)
        {
            _serializer = ServiceLocator.Current.Resolve<ISerializer<string>>();
            _serviceEntryLocate = ServiceLocator.Current.Resolve<IServiceEntryLocate>();
            _typeConvertibleService = ServiceLocator.Current.Resolve<ITypeConvertibleService>();
            _filters = filters;
        }

        public  Task OnActionExecuted(ActionExecutedContext filterContext)
        {
             return Task.CompletedTask;
        }

        public async Task OnActionExecuting(ActionExecutingContext filterContext)
        {
            filterContext.Context.Response.OnStarting(async () =>
           {
               var options = AppConfig.Options;
               var transferContract =  filterContext.Route.ServiceDescriptor.GetTransferContract();
               if (transferContract != null )
               {
                  
                   var option = options.Where(p => p.Name == transferContract.Name).FirstOrDefault();
                   if (option != null)
                   {
                       var routePath = string.IsNullOrEmpty(option.RoutePath) ? transferContract.RoutePath : option.RoutePath;

                       var address = new StringBuilder(option.Endpoint);
                       address = address.Append(routePath);
                       var serviceEntry = _serviceEntryLocate.Locate(filterContext.Message);
                       if (serviceEntry != null)
                       {
                           var parameters = new Dictionary<string, object>();
                           foreach (var parameterInfo in serviceEntry.Parameters)
                           {
                               var value = filterContext.Message.Parameters[parameterInfo.Name];
                               var parameterType = parameterInfo.ParameterType;
                               var parameter = _typeConvertibleService.Convert(value, parameterType);
                               parameters.Add(parameterInfo.Name, _typeConvertibleService.Convert(value, parameterType)); 
                           }
                           try
                           {
                               var result = await ServiceLocator.GetService<ITransportClient>(transferContract.Type.ToString()).SendAsync(address.ToString(),
                                 parameters, filterContext.Context); 
                               await new MessageSender(_serializer, filterContext.Context).SendAndFlushAsync(result, GetContentType(transferContract.Type));
                           }
                           catch(Exception ex)
                           {
                               var i = 0;
                           }

                         
                       }
                   }
                 
               }

           });
        }

        private string GetContentType(TransferContractType type)
        {
            var result = "";
            switch (type)
            {
                case TransferContractType.Rest:
                    {
                        result = "application/json;charset=utf-8";

                    } break;
                case TransferContractType.WebService:
                    {
                        result = "text/xml;charset=utf-8";

                    } break;
            }
            return result;
        }
    }
}
