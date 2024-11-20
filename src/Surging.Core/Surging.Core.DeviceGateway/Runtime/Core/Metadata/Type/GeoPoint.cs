using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Core.Metadata.Type
{
    public class GeoPoint
    {
        //经度
        private readonly double _lon;

        //纬度
        private readonly double _lat;

        public GeoPoint(double lon, double lat)
        {
            _lat = lat;
            _lon = lon;
        }

        public int hashCode()
        {

            int result = 1;
            long temp = BitConverter.DoubleToInt64Bits(_lat);
            result = 31 * result + (int)(temp ^ temp >> 32);

            temp = BitConverter.DoubleToInt64Bits(_lon);
            result = 31 * result + (int)(temp ^ temp >> 32);

            return result;
        }

        public static GeoPoint Instance(object val)
        {
            if (val == null)
            {
                return null;
            }
            object tmp = val;
            if (val is GeoPoint)
            {
                return (GeoPoint)val;
            }
            if (val is string)
            {
                var strVal = val.ToString();
                if (strVal.StartsWith("{"))
                {
                    // {"lon":"lon","lat":lat}
                    val = JsonSerializer.Deserialize<JsonObject>(strVal);
                }
                else if (strVal.StartsWith("["))
                {
                    // [lon,lat]
                    val = JsonSerializer.Deserialize<JsonArray>(strVal);
                }
                else
                {
                    // lon,lat
                    val = strVal.Split(",");
                }
            }
            if (val is Dictionary<object, object>)
            {
                var mapVal = (Dictionary<object, object>)val;
                object? lon = mapVal.GetValueOrDefault("lon", mapVal.GetValueOrDefault("y"));
                object? lat = mapVal.GetValueOrDefault("lat", mapVal.GetValueOrDefault("x"));
                val = new object[] { lon, lat };
            }
            if (val is ICollection<object>)
            {
                val = ((ICollection<object>)val).ToArray();
            }
            if (val is object[])
            {
                object[] arr = (object[])val;
                if (arr.Length >= 2)
                {
                    return new GeoPoint(double.Parse(arr[0].ToString()), double.Parse(arr[1].ToString()));
                }
            }
            throw new ArgumentException("unsupported geo format:" + tmp);
        }

        public override bool Equals(object obj)
        {
            if (this == obj)
            {
                return true;
            }

            if (!(obj is GeoPoint))
            {
                return false;
            }

            GeoPoint other = (GeoPoint)obj;

            return BitConverter.DoubleToInt64Bits(_lon) == BitConverter.DoubleToInt64Bits(other._lon) &&
                    BitConverter.DoubleToInt64Bits(_lat) == BitConverter.DoubleToInt64Bits(other._lat);
        }

        public override string ToString()
        {
            return $"{_lon},{_lat}";
        }
    }
}
