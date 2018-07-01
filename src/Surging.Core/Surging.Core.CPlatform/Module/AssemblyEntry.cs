using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Surging.Core.CPlatform.Module
{
    [XmlRoot("Assembly")]
    public class AssemblyEntry : IXmlSerializable
    {
        #region 实例属性

        /// <summary>
        /// 获取程序集文件名。
        /// </summary>
        public string FileName
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取程序集完全名称。
        /// </summary>
        public string FullName
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取程序集名称唯一键。
        /// </summary>
        public string Name
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取程序集标题文本。
        /// </summary>
        /// <value>
        /// 标题文本。
        /// </value>
        public string Title
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取程序集功能描述。
        /// </summary>
        /// <value>
        /// 程序集功能描述文本。
        /// </value>
        public string Description
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取程序集版本号。
        /// </summary>

        public Version Version
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取一个值指示该程序集是否是扩展的程序集。
        /// </summary>
        /// <value>
        /// 如果 <c>true</c> 这个程序集是扩展的；否则，<c>false</c> 不是扩展的程序集。
        /// </value>
        public bool IsExtend
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取一个值指示是否禁止停止和卸载。
        /// </summary>
        /// <value>
        /// 如果 <c>true</c> 这个程序集将禁止停止和卸载；否则，<c>false</c> 允许停止和卸载。
        /// </value>
        public bool DisableStopAndUninstalled
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取程序集排序顺序。
        /// </summary>
        /// <value>
        /// 从 1 开始的整数数值。
        /// </value>
        public int ListOrder { get; set; }

        /// <summary>
        /// 获取程序集模块状态。
        /// </summary>
        /// <value>
        /// 程序集模块状态枚举值：Installed | Start | Stop | Uninstalled 。
        /// </value>
        public ModuleState State { get; set; }

        /// <summary>
        /// 获取程序集引用列表。
        /// </summary>
        public List<string> Reference
        {
            get;
            internal set;
        }

        /// <summary>
        /// 获取程序集模块列表。
        /// </summary>
        /// <value>
        /// 程序集模块列表 List 泛型集合。
        /// </value>
        public List<AbstractModule> AbstractModules
        {
            get;
            set;
        }

        #endregion

        #region 实例方法

        /// <summary>
        /// 注册程序集所有抽象模块。
        /// </summary>
        /// <param name="builder">容器构建对象。</param>
        public void RegisterModule(ContainerBuilderWrapper builder)
        {
            AbstractModules.ForEach(module =>
            {
             //   module.Initialize();
                builder.RegisterModule(module);
            });
        }

        /// <summary>
        /// 获取指定控制器类型的业务模块实例。
        /// </summary>
        public BusinessModule GetBusinessModuleByControllerType(Type controllerType)
        {
            BusinessModule businessModule = this.AbstractModules.Find(m => controllerType.Namespace.Contains(m.GetType().Namespace) == true) as BusinessModule;

            if (businessModule == null)
            {
                throw new Exception(string.Format("无法找到 {0} 控制器所属的业务模块实例对象", controllerType.Name));
            }

            return businessModule;
        }
       
        /// <summary>
        /// 获取程序集视图目录名称：公司.产品.程序集模块(程序集视图目录名称)。
        /// </summary>
        /// <returns>返回程序集视图目录名称</returns>
        public string GetAssemblyViewDirectoryName()
        {
            return this.Name.Substring(this.Name.LastIndexOf('.') + 1);
        }

        /// <summary>
        /// 比较程序集模块版本。
        /// </summary>
        /// <param name="version">版本字符串。</param>
        /// <returns>
        ///     <para>一个有符号整数，它指示两个对象的相对值，如下表所示。返回值含义小于零当前的 System.Version 对象是 version 之前的一个版本。</para>
        ///     <para>零当前的System.Version 对象是与 version 相同的版本。大于零当前 System.Version 对象是 version 之后的一个版本。-或 -version 为 null。</para>
        /// </returns>
        public int CompareVersion(string version)
        {
            return CompareVersion(new Version(version));
        }

        /// <summary>
        /// 比较程序集模块版本。
        /// </summary>
        /// <param name="version">版本对象。</param>
        /// <returns>
        ///     <para>一个有符号整数，它指示两个对象的相对值，如下表所示。返回值含义小于零当前的 System.Version 对象是 version 之前的一个版本。</para>
        ///     <para>零当前的System.Version 对象是与 version 相同的版本。大于零当前 System.Version 对象是 version 之后的一个版本。-或 -version 为 null。</para>
        /// </returns>
        public int CompareVersion(Version version)
        {
            return Version.CompareTo(version);
        }

        /// <summary>
        /// 获取程序集模块的字符串文本描述信息。
        /// </summary>
        /// <returns>
        /// 返回程序集模块对象的字符串文本描述信息。
        /// </returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("程序集文件名：{0}", FileName);
            sb.AppendLine();

            sb.AppendFormat("程序集类型全名：{0}", FullName);
            sb.AppendLine();

            sb.AppendFormat("程序集类型名：{0}", Name);
            sb.AppendLine();

            sb.AppendFormat("程序集标题：{0}", Title);
            sb.AppendLine();

            sb.AppendFormat("程序集描述：{0}", Description);
            sb.AppendLine();

            sb.AppendFormat("程序集版本：{0}", Version);
            sb.AppendLine();

            sb.AppendFormat("扩展的程序集：{0}", IsExtend);
            sb.AppendLine();

            sb.AppendFormat("禁用停止和卸载：{0}", DisableStopAndUninstalled);
            sb.AppendLine();

            sb.AppendFormat("程序集注册顺序：{0}", ListOrder);
            sb.AppendLine();

            sb.AppendFormat("程序集引用 {0}个", Reference.Count);
            sb.AppendLine();

            Reference.ForEach(r =>
            {
                sb.AppendLine(r);
            });

            sb.AppendFormat("程序集模块 {0}个", AbstractModules.Count);
            sb.AppendLine();

            AbstractModules.ForEach(m =>
            {
                sb.Append(m.ToString());
            });

            return sb.ToString();
        }

        #region IXmlSerializable 成员

        /// <summary>
        /// 此方法是保留方法，请不要使用。在实现 IXmlSerializable 接口时，应从此方法返回 null（在 Visual Basic 中为 Nothing），如果需要指定自定义架构，应向该类应用 <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute"/>。
        /// </summary>
        /// <returns>
        ///   <see cref="T:System.Xml.Schema.XmlSchema"/>，描述由 <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)"/> 方法产生并由 <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)"/> 方法使用的对象的 XML 表示形式。
        /// </returns>
        public XmlSchema GetSchema()
        {
            return null;
        }

        /// <summary>
        /// 从对象的 XML 表示形式生成该对象。
        /// </summary>
        /// <param name="reader">对象从中进行反序列化的 <see cref="T:System.Xml.XmlReader"/> 流。</param>

        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement || !reader.Read())
            {
                return;
            }

            while (reader.NodeType != XmlNodeType.EndElement)
            {
                this.FileName = reader.ReadElementContentAsString();
                this.FullName = reader.ReadElementContentAsString();
                this.Name = reader.ReadElementContentAsString();
                this.Title = reader.ReadElementContentAsString();
                this.Description = reader.ReadElementContentAsString();

                System.Version version;
                System.Version.TryParse(reader.ReadElementContentAsString(), out version);
                this.Version = version;

                bool isExtend;
                bool.TryParse(reader.ReadElementContentAsString(), out isExtend);
                this.IsExtend = isExtend;

                bool disableStopAndUninstalled;
                bool.TryParse(reader.ReadElementContentAsString(), out disableStopAndUninstalled);
                this.DisableStopAndUninstalled = disableStopAndUninstalled;

                int listOrder;
                int.TryParse(reader.ReadElementContentAsString(), out listOrder);
                this.ListOrder = listOrder;

                ModuleState state;
                Enum.TryParse<ModuleState>(reader.ReadElementContentAsString(), out state);
                this.State = state;

                this.Reference = new List<string>();
                if (reader.Name == "Reference")
                {
                    if (!reader.IsEmptyElement)
                    {
                        reader.ReadStartElement("Reference");
                        while (reader.Name == "Assembly")
                        {
                            this.Reference.Add(reader.ReadElementContentAsString());
                        }
                        reader.ReadEndElement();
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                this.AbstractModules = new List<AbstractModule>();
                if (reader.Name == "AbstractModules")
                {
                    if (!reader.IsEmptyElement)
                    {
                        reader.ReadStartElement("AbstractModules");
                        while (reader.Name.EndsWith("Module"))
                        {
                            Type type = Type.GetType(reader.GetAttribute("TypeName"), true);
                            XmlSerializer xmlSerializer = new XmlSerializer(type);
                            this.AbstractModules.Add((AbstractModule)xmlSerializer.Deserialize(reader));
                        }
                        reader.ReadEndElement();
                    }
                    else
                    {
                        reader.Skip();
                    }
                }
            }
        }

        /// <summary>
        /// 将对象转换为其 XML 表示形式。
        /// </summary>
        /// <param name="writer">对象要序列化为的 <see cref="T:System.Xml.XmlWriter"/> 流。</param>
        public void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("FileName", this.FileName);
            writer.WriteElementString("FullName", this.FullName);
            writer.WriteElementString("Name", this.Name);
            writer.WriteElementString("Title", this.Title);
            writer.WriteElementString("Description", this.Description);
            writer.WriteElementString("Version", this.Version == null ? string.Empty : this.Version.ToString());
            writer.WriteElementString("IsExtend", this.IsExtend.ToString().ToLower());
            writer.WriteElementString("DisableStopAndUninstalled", this.DisableStopAndUninstalled.ToString().ToLower());
            writer.WriteElementString("ListOrder", this.ListOrder.ToString());
            writer.WriteElementString("State", this.State.ToString());

            writer.WriteStartElement("Reference");
            if (this.Reference != null)
            {
                this.Reference.ForEach(reference =>
                {
                    writer.WriteElementString("Assembly", reference);
                });
            }
            writer.WriteEndElement();

            writer.WriteStartElement("AbstractModules");
            if (this.AbstractModules != null)
            {
                this.AbstractModules.ForEach(module =>
                {
                    XmlSerializerNamespaces namespaces = new XmlSerializerNamespaces();
                    namespaces.Add(string.Empty, string.Empty);
                    XmlSerializer xmlSerializer = new XmlSerializer(module.GetType());
                    xmlSerializer.Serialize(writer, module, namespaces);
                });
            }
            writer.WriteEndElement();
        }

        #endregion

        #endregion
    }
}

