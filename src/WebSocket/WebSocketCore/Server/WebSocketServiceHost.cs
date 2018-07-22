using System; 

namespace WebSocketCore.Server
{
    public class WebSocketServiceHost : WebSocketServiceHostBase
    {
        private readonly WebSocketBehavior _webSocketBehavior;
        public override Type BehaviorType
        {
            get
            {
                return _webSocketBehavior.GetType();
            }
        }

        internal WebSocketServiceHost(string path, WebSocketBehavior webSocketBehavior, Logger log)
                : base(path, log)
        {
            _webSocketBehavior = webSocketBehavior;
        }

        protected override WebSocketBehavior CreateSession()
        {
            return _webSocketBehavior;
        }


    }
} 