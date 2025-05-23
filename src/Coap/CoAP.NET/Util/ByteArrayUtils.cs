/*
 * Copyright (c) 2011-2014, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;
using System.Text;

namespace CoAP.Util
{
    /// <summary>
    /// Utility methods for bytes array.
    /// </summary>
    public static class ByteArrayUtils
    {
        const String digits = "0123456789ABCDEF";

        /// <summary>
        /// Returns a hex string representation of the given bytes array.
        /// </summary>
        public static String ToHexString(Byte[] data)
        {
            if (data != null && data.Length > 0)
            {
                StringBuilder builder = new StringBuilder(data.Length * 3);
                for (Int32 i = 0; i < data.Length; i++)
                {
                    builder.Append(digits[(data[i] >> 4) & 0xF]);
                    builder.Append(digits[data[i] & 0xF]);
                }
                return builder.ToString();
            }
            else
            {
                return String.Empty;
            }
        }

        /// <summary>
        /// Parses a bytes array from its hex string representation.
        /// </summary>
        public static Byte[] FromHexStream(String hex)
        {
            try
            {
                hex = hex.Replace("\"", String.Empty);
                if (hex.Length % 2 == 1)
                    hex = "0" + hex;

                Byte[] tmp = new Byte[hex.Length / 2];
                for (Int32 i = 0, j = 0; i < hex.Length; i += 2)
                {
                    Int16 high = Int16.Parse(hex[i].ToString(), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                    Int16 low = Int16.Parse(hex[i + 1].ToString(), System.Globalization.NumberStyles.HexNumber, System.Globalization.CultureInfo.InvariantCulture);
                    tmp[j++] = Convert.ToByte(high * 16 + low);
                }

                return tmp;
            }
            catch { return null; }
        }

        /// <summary>
        /// Checks if the two bytes arrays are equal.
        /// </summary>
        public static Boolean Equals(Byte[] bytes1, Byte[] bytes2)
        {
            if (bytes1 == null && bytes2 == null)
                return true;
            else if (bytes1 == null || bytes2 == null || bytes1.Length != bytes2.Length)
                return false;
            for (Int32 i = 0; i < bytes1.Length; i++)
            {
                if (bytes1[i] != bytes2[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Computes the hash of the given bytes array.
        /// </summary>
        public static Int32 ComputeHash(params Byte[] data)
        {
            unchecked
            {
                const Int32 p = 16777619;
                Int32 hash = (Int32)2166136261;

                for (Int32 i = 0; i < data.Length; i++)
                    hash = (hash ^ data[i]) * p;

                hash += hash << 13;
                hash ^= hash >> 7;
                hash += hash << 3;
                hash ^= hash >> 17;
                hash += hash << 5;
                return hash;
            }
        }
    }
}
