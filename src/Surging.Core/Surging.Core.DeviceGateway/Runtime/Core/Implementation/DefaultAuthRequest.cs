 

namespace Surging.Core.DeviceGateway.Runtime.Core.Implementation
{
    public class DefaultAuthRequest : IAuthenticationRequest
    {

        public string UserName { get; }

        public string Password { get; }

        public string DeviceId { get; }


        private MessageTransport _transport;

        public DefaultAuthRequest(string password, string deviceId):this(null,password,deviceId)
        { 
        }

        public DefaultAuthRequest(string userName, string password, string deviceId, MessageTransport transport = MessageTransport.Tcp)
        {
            UserName = userName;
            Password = password;
            DeviceId = deviceId;
            _transport = transport;
        }

        public MessageTransport GetTransport()
        {
            return _transport;
        }
    }
}
