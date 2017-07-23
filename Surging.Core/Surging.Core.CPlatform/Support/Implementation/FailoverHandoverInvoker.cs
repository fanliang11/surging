using Surging.Core.CPlatform.Convertibles;
using Surging.Core.CPlatform.Messages;
using Surging.Core.CPlatform.Runtime.Client;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.CPlatform.Support.Implementation
{
   public class FailoverHandoverInvoker: IClusterInvoker
    {
        #region Field

        private readonly IRemoteInvokeService _remoteInvokeService;
        private readonly ITypeConvertibleService _typeConvertibleService;
        private readonly IBreakeRemoteInvokeService _breakeRemoteInvokeService;
        #endregion Field

        #region Constructor

        public FailoverHandoverInvoker(IRemoteInvokeService remoteInvokeService, ITypeConvertibleService typeConvertibleService, IBreakeRemoteInvokeService breakeRemoteInvokeService)
        {
            _remoteInvokeService = remoteInvokeService;
            _typeConvertibleService = typeConvertibleService;
            _breakeRemoteInvokeService = breakeRemoteInvokeService;
        }

        #endregion Constructor
        public async Task<T> Invoke<T>(IDictionary<string, object> parameters, string serviceId, string _serviceKey)
        {
            var time=0;
            T result=default(T);
            RemoteInvokeResultMessage message=new RemoteInvokeResultMessage();
            while (await  _breakeRemoteInvokeService.InvokeAsync(parameters,serviceId,_serviceKey) == null && ++time<3)
            {
                if (message.Result != null)
                 result =(T) _typeConvertibleService.Convert(message.Result, typeof(T)) ;
            } 
            return result;
        }

        public async Task Invoke(IDictionary<string, object> parameters, string serviceId, string _serviceKey)
        {
            var time = 0;
            RemoteInvokeResultMessage message = new RemoteInvokeResultMessage();
            while (await _breakeRemoteInvokeService.InvokeAsync(parameters, serviceId, _serviceKey) == null && ++time < 3) ;
        }

        private async Task<bool> IsHealth(IDictionary<string, object> parameters, string serviceId, string _serviceKey, RemoteInvokeResultMessage message)
        {
            bool result = true;
            try
            {
                message = await _remoteInvokeService.InvokeAsync(new RemoteInvokeContext
                {
                    InvokeMessage = new RemoteInvokeMessage
                    {
                        Parameters = parameters,
                        ServiceId = serviceId,
                        ServiceKey = _serviceKey
                    }
                });
                return result;
            }
            catch
            {
                return false;
            }
        }
    }

}
