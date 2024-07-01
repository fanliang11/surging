using System;
using System.Collections.Generic;
using System.Net;

namespace Surging.Core.Protocol.Tcp.Runtime
{
    public class TcpServerProperties
    {
        public string _id;
        public string Id
        {
            get { return _id; }
            set { _id = value; }
        }


        private PayloadType _payloadType;
        public PayloadType PayloadType
        {
            get { return _payloadType; }
            set { _payloadType = value; }
        }

        private IDictionary<string, object> _parserConfiguration;
        public IDictionary<string, object> ParserConfiguration
        {
            get { return _parserConfiguration; }
            set { _parserConfiguration = value; }
        }

        private PayloadParserType _parserType;
        public PayloadParserType ParserType
        {
            get { return _parserType; }
            set { _parserType = value; }
        } 

        private string _host;
        public string Host
        {
            get { return _host; }
            set { _host = value; }
        }

        private int _port;
        public int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        private bool  _ssl;
        public bool SSL
        {
            get { return _ssl; }
            set { _ssl = value; }
        }

        //服务实例数量(线程数)
        private int _instance = Environment.ProcessorCount;
        public int Instance
        {
            get { return _instance; }
            set { _instance = value; }
        }

        private string _certId;
        public string CertId
        {
            get { return _certId; }
            set { _certId = value; }
        }

        public IPEndPoint CreateSocketAddress()
        {
            if (string.IsNullOrEmpty(_host))
            {
                _host = "localhost";
            }
            return new IPEndPoint(IPAddress.Parse(_host),_port);
        }
    }
}
