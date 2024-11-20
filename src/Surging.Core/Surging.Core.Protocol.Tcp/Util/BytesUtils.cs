using DotNetty.Buffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.Util
{
    public class BytesUtils
    {
        public  IByteBuffer Slice(IByteBuffer byteBuffer,int index,int length)
        {
            var result=  byteBuffer.RetainedSlice(index, length).SetReaderIndex(0);
            byteBuffer.SetReaderIndex(length+index);
            result.SetReaderIndex(0);
            return result;
        }

        public   int LeToInt(byte[] src, int offset, int len)
        {
            int n = 0;
            len = Math.Min(len, 4);
            for (int i = 0; i < len; i++)
            {
                int left = i * 8;
                n += ((src[i + offset] & 0xFF) << left);
            }
            return n;
        }

        public int LeStrToInt(byte[] src, int offset, int len)
        {
            string n = "";
            len = Math.Min(len, 4);
            for (int i = 0; i < len; i++)
            {
              if( int.TryParse( Encoding.Default.GetString(new byte[] { src[i] }),out int result))
            
                n += result.ToString();
            }
            return int.Parse(n);
        }

        public byte[]  GetBytes(IByteBuffer buffer, int srcIndex, int length)
        { 
            var result= new byte[length];
            buffer.SkipBytes(srcIndex).ReadBytes(result); 
            return result;
        }


        public List<byte> GetEmptyBytes()
        { 
            return Array.Empty<byte>().ToList();
        }

        public byte[] GetBytes(int len)
        {
            return new byte[len];
        }


        /**
         * 高位字节数组转int,低字节在前.
         * -------------------------------------------
         * |  0-7位  |  8-16位  |  17-23位  |  24-31位 |
         * -------------------------------------------
         *
         * @param src 字节数组
         * @return int值
         */
        public int HighBytesToInt(byte[] src)
        {
            return HighBytesToInt(src, 0, src.Length);
        }

        /**
         * 高位字节数组转long,低字节在前.
         * -------------------------------------------
         * |  0-7位  |  8-16位  |  17-23位  |  24-31位 |
         * -------------------------------------------
         *
         * @param src 字节数组
         * @return int值
         */
        public  int HighBytesToInt(byte[] src, int offset, int len)
        {
            int n = 0;
            len = Math.Min(len, 4);
            for (int i = 0; i < len; i++)
            {
                int left = i * 8;
                n += ((src[i + offset] & 0xFF) << left);
            }
            return n;
        }


        public  long HighBytesToLong(byte[] src, int offset, int len)
        {
            long n = 0;
            len = Math.Min(Math.Min(len, src.Length), 8);
            for (int i = 0; i < len; i++)
            {
                int left = i * 8;
                n += ((long)(src[i + offset] & 0xFF) << left);
            }
            return n;
        }

        /**
         * 低位字节数组转int,低字节在后.
         * -------------------------------------------
         * |  31-24位 |  23-17位   |  16-8位 |   7-0位 |
         * -------------------------------------------
         *
         * @param src 字节数组
         * @return int值
         */
        public  int LowBytesToInt(byte[] src, int offset, int len)
        {
            int n = 0;
            len = Math.Min(len, 4);
            for (int i = 0; i < len; i++)
            {
                int left = i * 8;
                n += ((src[offset + len - i - 1] & 0xFF) << left);
            }
            return n;
        }

        /**
         * 低位字节数组转long,低字节在后.
         * -------------------------------------------
         * |  31-24位 |  23-17位   |  16-8位 |   7-0位 |
         * -------------------------------------------
         *
         * @param src 字节数组
         * @return int值
         */
        public long LowBytesToLong(byte[] src, int offset, int len)
        {
            long n = 0;
            len = Math.Min(len, 4);
            for (int i = 0; i < len; i++)
            {
                int left = i * 8;
                n += ((long)(src[offset + len - i - 1] & 0xFF) << left);
            }
            return n;
        }

        /**
         * 低位字节数组转int,低字节在后
         * -------------------------------------------
         * |  31-24位 |  23-17位   |  16-8位 |   7-0位 |
         * -------------------------------------------
         *
         * @param src 字节数组
         * @return int值
         */
        public  int LowBytesToInt(byte[] src)
        { 
            return LowBytesToInt(src, 0, src.Length);
        }

        /**
         * int转高位字节数组,低字节在前
         * -------------------------------------------
         * |  0-7位  |  8-16位  |  17-23位  |  24-31位 |
         * -------------------------------------------
         *
         * @param src 字节数组
         * @return bytes 值
         */
        public  byte[] ToHighBytes(byte[] target, long src, int offset, int len)
        {
            for (int i = 0; i < len; i++)
            {
                target[offset + i] = (byte)(src >> (i * 8) & 0xff);
            }
            return target;
        }


        /**
         * int转高位字节数组,低字节在前
         * -------------------------------------------
         * |  0-7位  |  8-16位  |  17-23位  |  24-31位 |
         * -------------------------------------------
         *
         * @param src 字节数组
         * @return bytes 值
         */
        public  byte[] ToHighBytes(int src)
        {
            return ToHighBytes(new byte[4], src, 0, 4);
        }


        /**
         * 转低位字节数组, 低字节在后
         * --------------------------------------------
         * |  31-24位 |  23-17位   |  16-8位 |   7-0位 |
         * --------------------------------------------
         *
         * @param src 字节数组
         * @return int值
         */
        public  byte[] ToLowBytes(byte[] target, long src, int offset, int len)
        {
            for (int i = 0; i < len; i++)
            {
                target[offset + len - i - 1] = (byte)(src >> (i * 8) & 0xff);
            }
            return target;
        }

        /**
         * int转低位字节数组, 低字节在后
         * --------------------------------------------
         * |  31-24位 |  23-17位   |  16-8位 |   7-0位 |
         * --------------------------------------------
         *
         * @param src 字节数组
         * @return int值
         */
        public  byte[] ToLowBytes(int src)
        {
            return ToLowBytes(new byte[4], src, 0, 4);
        }

        public  byte[] ToLowBytes(long src)
        {
            return ToLowBytes(new byte[8], src, 0, 8);
        }
    }
}
