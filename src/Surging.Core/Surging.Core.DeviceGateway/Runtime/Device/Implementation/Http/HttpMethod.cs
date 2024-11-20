using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.Device.Implementation.Http
{
    public class HttpMethod : IEquatable<HttpMethod>
    {
        public HttpMethod(string method)
        {
            Method = method;
        }

        public static HttpMethod Delete { get;}= new HttpMethod("Delete");
        public static HttpMethod Get { get; } = new HttpMethod("Get");

        public static HttpMethod Head { get; } = new HttpMethod("Head");

        public static HttpMethod Options { get; } = new HttpMethod("Options");

        public static HttpMethod Patch { get; } = new HttpMethod("Patch");

        public static HttpMethod Post { get; } = new HttpMethod("Post");

        public static HttpMethod Put { get; } = new HttpMethod("Put");

        public static HttpMethod Trace { get; } = new HttpMethod("Trace");

        public string Method { get; }


        public bool Equals([NotNullWhen(true)] HttpMethod? other)
        {
            return Method.Equals(other?.Method, StringComparison.OrdinalIgnoreCase);
        }

        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            var other = obj as HttpMethod;
            return Method.Equals(other?.Method, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return Method.GetHashCode(StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString()
        {
            return Method;
        }

        public static bool operator ==(HttpMethod? left, HttpMethod? right)
        {
            return left?.Method.Equals(right?.Method, StringComparison.OrdinalIgnoreCase) ?? false;

        }

        public static bool operator !=(HttpMethod? left, HttpMethod? right)
        {
            return !(left?.Method.Equals(right?.Method, StringComparison.OrdinalIgnoreCase) ?? false);
        }
    }
}