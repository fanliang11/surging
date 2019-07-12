using System;

namespace WebSocketCore.Server
{
    /// <summary>
    /// Defines the <see cref="WebSocketServiceHost" />
    /// </summary>
    public class WebSocketServiceHost : WebSocketServiceHostBase
    {
        #region 字段

        /// <summary>
        /// Defines the _webSocketBehavior
        /// </summary>
        private readonly WebSocketBehavior _webSocketBehavior;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServiceHost"/> class.
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="webSocketBehavior">The webSocketBehavior<see cref="WebSocketBehavior"/></param>
        /// <param name="log">The log<see cref="Logger"/></param>
        internal WebSocketServiceHost(string path, WebSocketBehavior webSocketBehavior, Logger log)
                : base(path, log)
        {
            _webSocketBehavior = webSocketBehavior;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the BehaviorType
        /// </summary>
        public override Type BehaviorType
        {
            get
            {
                return _webSocketBehavior.GetType();
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The CreateSession
        /// </summary>
        /// <returns>The <see cref="WebSocketBehavior"/></returns>
        protected override WebSocketBehavior CreateSession()
        {
            return _webSocketBehavior;
        }

        #endregion 方法
    }
}