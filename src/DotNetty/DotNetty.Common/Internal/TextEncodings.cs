namespace DotNetty.Common.Internal
{
    using System;
    using System.Runtime.CompilerServices;
    using System.Text;

    public static partial class TextEncodings
    {
        /// <summary>不提供 Unicode 字节顺序标记，检测到无效的编码时不引发异常</summary>
        public static readonly UTF8Encoding UTF8NoBOM = new(false);

        /// <summary>不提供 Unicode 字节顺序标记，检测到无效的编码时引发异常</summary>
        public static readonly UTF8Encoding SecureUTF8NoBOM = new(false, true);

        /// <summary>提供 Unicode 字节顺序标记，检测到无效的编码时引发异常</summary>
        public static readonly UTF8Encoding SecureUTF8 = new(true, true);

        public const int ASCIICodePage = 20127;

        public const int ISO88591CodePage = 28591;

        public const int UTF8CodePage = 65001;

        // Encoding Helpers
        private const char HighSurrogateStart = '\ud800';
        private const char HighSurrogateEnd = '\udbff';
        private const char LowSurrogateStart = '\udc00';
        private const char LowSurrogateEnd = '\udfff';

#if !NETCOREAPP_3_0_GREATER

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static int PtrDiff(char* a, char* b)
        {
            return (int)(((uint)((byte*)a - (byte*)b)) >> 1);
        }

        // byte* flavor just for parity
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private unsafe static int PtrDiff(byte* a, byte* b)
        {
            return (int)(a - b);
        }

#endif
    }
}
