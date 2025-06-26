using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class GeoType : IDataType, IConverter<GeoPoint>, IConverter<object>
    {
        public readonly string id = "geoPoint";

        public readonly string name = "地理位置";

        public object Format(string format, object value)
        {
            GeoPoint geoPoint = Convert(value);
            return geoPoint.ToString();
        }

        public string GetId()
        {
            return id;
        }

        public string GetName()
        {
            return name;
        }

        public bool Validate(object value)
        {
            GeoPoint geoPoint = Convert(value);
            return geoPoint == null
                    ? false
                    : true;
        }

        public GeoPoint Convert(object value)
        {
            return GeoPoint.Instance(value);
        }

        object IConverter<object>.Convert(object value)
        {
            return Convert(value);
        }
    }
}
