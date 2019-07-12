using Surging.Core.CPlatform.Utilities;
using Surging.Core.Protocol.WS;
using Surging.Core.Protocol.WS.Runtime;
using Surging.Core.ProxyGenerator;
using Surging.IModuleServices.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WebSocketCore;

namespace Surging.Modules.Common.Domain
{
    /// <summary>
    /// Defines the <see cref="ChatService" />
    /// </summary>
    public class ChatService : WSServiceBase, IChatService
    {
        #region 字段

        /// <summary>
        /// Defines the _clients
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _clients = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Defines the _users
        /// </summary>
        private static readonly ConcurrentDictionary<string, string> _users = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// Defines the _name
        /// </summary>
        private string _name;

        /// <summary>
        /// Defines the _to
        /// </summary>
        private string _to;

        #endregion 字段

        #region 方法

        /// <summary>
        /// The SendMessage
        /// </summary>
        /// <param name="name">The name<see cref="string"/></param>
        /// <param name="data">The data<see cref="string"/></param>
        /// <returns>The <see cref="Task"/></returns>
        public Task SendMessage(string name, string data)
        {
            if (_users.ContainsKey(name))
            {
                this.GetClient().SendTo($"hello,{name},{data}", _users[name]);
            }
            return Task.CompletedTask;
        }

        /// <summary>
        /// The OnMessage
        /// </summary>
        /// <param name="e">The e<see cref="MessageEventArgs"/></param>
        protected override void OnMessage(MessageEventArgs e)
        {
            if (_clients.ContainsKey(ID))
            {
                Dictionary<string, object> model = new Dictionary<string, object>();
                model.Add("name", _to);
                model.Add("data", e.Data);
                var result = ServiceLocator.GetService<IServiceProxyProvider>()
                     .Invoke<object>(model, "api/chat/SendMessage").Result;
            }
        }

        /// <summary>
        /// The OnOpen
        /// </summary>
        protected override void OnOpen()
        {
            _name = Context.QueryString["name"];
            _to = Context.QueryString["to"];
            if (!string.IsNullOrEmpty(_name))
            {
                _clients[ID] = _name;
                _users[_name] = ID;
            }
        }

        #endregion 方法
    }
}