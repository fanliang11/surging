using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Surging.Core.Protokollwandler.Internal
{
   public interface IWebServiceProvider
    {
         XmlDocument QueryPostWebService(String url, String MethodName, IDictionary<string, object> Pars);

        /// <summary>
        /// 需要WebService支持Get调用
        /// </summary>
        XmlDocument QueryGetWebService(String url, String MethodName, IDictionary<string, object> Pars);

        /// <summary>
        /// 通用WebService调用(Soap),参数Pars为String类型的参数名、参数值
        /// </summary>
         XmlDocument QuerySoapWebService(String url, String MethodName, IDictionary<string,object> Pars);
         
    }
}
