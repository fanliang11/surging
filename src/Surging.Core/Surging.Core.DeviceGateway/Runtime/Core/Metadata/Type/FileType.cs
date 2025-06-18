using DotNetty.Common.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    internal class FileType : IDataType, IConverter<string?>, IConverter<object>
    {
        public readonly string _id = "file";

        private readonly string _name = "布尔值";

        private BodyTypeEnum _bodyType = BodyTypeEnum.Url;

        private string _mediaType = "application/octet-stream";

        public string GetId()
        {
            return _id;
        }

        public string GetName()
        {
            return _name;
        }

        public FileType BodyType(BodyTypeEnum type)
        {
            _bodyType = type;
            return this;
        }

        public FileType MediaType(string type)
        {
            if (!string.IsNullOrEmpty(type))
            {
                _mediaType = type;
            }
            return this;
        }

        public bool Validate(object value)
        {
            return string.IsNullOrEmpty(value?.ToString());
        }

        public object Format(string format, object value)
        {
            return string.Format(format, value);
        }

        public string? Convert(object value)
        {
            return value == null ? null : value.ToString();
        }

        object IConverter<object>.Convert(object value)
        {
            return Convert(value);
        }

        public enum BodyTypeEnum
        {
            Url,
            Base64,
            Binary
        }
    }
}
