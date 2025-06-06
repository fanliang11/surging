using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core
{
    public interface IDeviceGatewayProvider
    {
        string GetId();

        string GetName();


        string GetDescription();


        string GetChannel();



        /**
         * @return 传输协议
         */
        MessageTransport GetTransport();

        /**
         * 使用配置信息创建设备网关
         *
         * @param properties 配置
         * @return void
         */
        IObservable<IDeviceGateway> CreateDeviceGateway(DeviceGatewayProperties properties);

        /**
         * 重新加载网关
         *
         * @param gateway    网关
         * @param properties 配置信息
         * @return void
         */
        IObservable<IDeviceGateway> ReloadDeviceGateway(IDeviceGateway gateway, DeviceGatewayProperties properties);
    }
}
