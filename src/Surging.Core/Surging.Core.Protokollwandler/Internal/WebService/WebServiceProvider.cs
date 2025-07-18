using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.Serialization;

namespace Surging.Core.Protokollwandler.Internal.WebService
{
   public class WebServiceProvider: IWebServiceProvider
    {
        /// <summary>
        /// 需要WebService支持Post调用
        /// </summary>
        public  XmlDocument QueryPostWebService(string url, string methodName, IDictionary<string, object> pars)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url + "/" + methodName);
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            SetWebRequest(request);
            byte[] data = EncodePars(pars);
            WriteRequestData(request, data);
            return ReadXmlResponse(request.GetResponse());
        }

        /// <summary>
        /// 需要WebService支持Get调用
        /// </summary>
        public  XmlDocument QueryGetWebService(string url, string methodName, IDictionary<string, object> pars)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url + "/" + methodName + "?" + ParsToString(pars));
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            SetWebRequest(request);
            return ReadXmlResponse(request.GetResponse());
        }

        /// <summary>
        /// 通用WebService调用(Soap),参数pars为string类型的参数名、参数值
        /// </summary>
        public  XmlDocument QuerySoapWebService(string url, string methodName, IDictionary<string, object> pars)
        {
            if (_xmlNamespaces.ContainsKey(url))
            {
                return QuerySoapWebService(url, methodName, pars, _xmlNamespaces[url].ToString());
            }
            else
            {
                return QuerySoapWebService(url, methodName, pars, GetNamespace(url));
            }
        }

        private  XmlDocument QuerySoapWebService(string url, string methodName, IDictionary<string, object> pars, string XmlNs)
        {
            _xmlNamespaces[url] = XmlNs;//加入缓存，提高效率
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            request.Headers.Add("SOAPAction", "\"" + XmlNs + (XmlNs.EndsWith("/") ? "" : "/") + methodName + "\"");
            SetWebRequest(request);
            byte[] data = EncodeParsToSoap(pars, XmlNs, methodName);
            WriteRequestData(request, data);
            XmlDocument doc = new XmlDocument(), doc2 = new XmlDocument();
            doc = ReadXmlResponse(request.GetResponse());

            XmlNamespaceManager mgr = new XmlNamespaceManager(doc.NameTable);
            mgr.AddNamespace("soap", "http://schemas.xmlsoap.org/soap/envelope/");
            string RetXml = doc.SelectSingleNode("//soap:Body/*/*", mgr).InnerXml;
            doc2.LoadXml("<root>" + RetXml + "</root>");
            AddDelaration(doc2);
            return doc2;
        }
        private static string GetNamespace(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url + "?WSDL");
            SetWebRequest(request);
            WebResponse response = request.GetResponse();
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(sr.ReadToEnd());
            sr.Close();
            return doc.SelectSingleNode("//@targetNamespace").Value;
        }

        private static byte[] EncodeParsToSoap(IDictionary<string, object> pars, string XmlNs, string methodName)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"></soap:Envelope>");
            AddDelaration(doc); 
            XmlElement soapBody = doc.CreateElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/"); 
            XmlElement soapMethod = doc.CreateElement(methodName);
            soapMethod.SetAttribute("xmlns", XmlNs);
            foreach (string k in pars.Keys)
            { 
                XmlElement soapPar = doc.CreateElement(k);
                soapPar.InnerXml = ObjectToSoapXml(pars[k]);
                soapMethod.AppendChild(soapPar);
            }
            soapBody.AppendChild(soapMethod);
            doc.DocumentElement.AppendChild(soapBody);
            return Encoding.UTF8.GetBytes(doc.OuterXml);
        }

        private static byte[] EncodeParsToSoap(XmlNodeList xmlNodes, string XmlNs, string methodName)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<soap:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap=\"http://schemas.xmlsoap.org/soap/envelope/\"></soap:Envelope>");
            AddDelaration(doc); 
            XmlElement soapBody = doc.CreateElement("soap", "Body", "http://schemas.xmlsoap.org/soap/envelope/"); 
            XmlElement soapMethod = doc.CreateElement(methodName);
            soapMethod.SetAttribute("xmlns", XmlNs);
            foreach (XmlNode node in xmlNodes)
            {
                try
                {
                    XmlElement soapPar = doc.CreateElement(node.Name);
                    soapPar.InnerXml = node.OuterXml;
                    soapMethod.AppendChild(soapPar);
                }
                catch(Exception ex)
                {
                    var i = 0;
                }
            }
            soapBody.AppendChild(soapMethod);
            doc.DocumentElement.AppendChild(soapBody);
            return Encoding.UTF8.GetBytes(doc.OuterXml);
        }

        private static string ObjectToSoapXml(object o)
        {
            
            if (o.GetType() == typeof(JArray))
            {
                var jarray = o as JArray;
                var xml = new StringBuilder();
                foreach(var obj in jarray)
                {
                    xml.AppendFormat("<{0}>{1}</{0}>", obj.Type.ToString().ToLower(), obj);
                }
                return xml.ToString();
            }
            else
            {
                XmlSerializer mySerializer = new XmlSerializer(o.GetType());
                MemoryStream ms = new MemoryStream();
                mySerializer.Serialize(ms, o);
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Encoding.UTF8.GetString(ms.ToArray()));
                if (doc.DocumentElement != null)
                {
                    return doc.DocumentElement.InnerXml;
                }
                else
                {
                    return o.ToString();
                }
            }
        }

        /// <summary>
        /// 设置凭证与超时时间
        /// </summary>
        /// <param name="request"></param>
        private static void SetWebRequest(HttpWebRequest request)
        {
            request.Credentials = CredentialCache.DefaultCredentials;
            request.Timeout = 10000;
        }

        private static void WriteRequestData(HttpWebRequest request, byte[] data)
        {
            request.ContentLength = data.Length;
            Stream writer = request.GetRequestStream();
            writer.Write(data, 0, data.Length);
            writer.Close();
        }

        private static byte[] EncodePars(IDictionary<string, object> pars)
        {
            return Encoding.UTF8.GetBytes(ParsToString(pars));
        }

   
    private static String ParsToString(IDictionary<string, object> Pars)
    {
        StringBuilder sb = new StringBuilder();
        foreach (string k in Pars.Keys)
        {
            if (sb.Length > 0)
            {
                sb.Append("&");
            }
            sb.Append(HttpUtility.UrlEncode(k) + "=" + HttpUtility.UrlEncode(Pars[k].ToString()));
        }
        return sb.ToString();
    } 

        private static XmlDocument ReadXmlResponse(WebResponse response)
        {
            StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
            String retXml = sr.ReadToEnd();
            sr.Close();
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(retXml);
            return doc;
        }

        private static void AddDelaration(XmlDocument doc)
        {
            XmlDeclaration decl = doc.CreateXmlDeclaration("1.0", "utf-8", null);
            doc.InsertBefore(decl, doc.DocumentElement);
        }

     

        private static IDictionary<string, object> _xmlNamespaces = new Dictionary<string, object>();//缓存xmlNamespace，避免重复调用GetNamespace
    }
}
