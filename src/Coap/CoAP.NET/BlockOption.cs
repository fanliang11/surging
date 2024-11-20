/*
 * Copyright (c) 2011-2012, Longxiang He <helongxiang@smeshlink.com>,
 * SmeshLink Technology Co.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY.
 * 
 * This file is part of the CoAP.NET, a CoAP framework in C#.
 * Please see README for more information.
 */

using System;

namespace CoAP
{
    /// <summary>
    /// This class describes the block options of the CoAP messages
    /// </summary>
    public class BlockOption : Option
    {
        /// <summary>
        /// Initializes a block option.
        /// </summary>
        /// <param name="type">The type of the option</param>
        public BlockOption(OptionType type) : base(type)
        {
            this.IntValue = 0;
        }

        /// <summary>
        /// Initializes a block option.
        /// </summary>
        /// <param name="type">The type of the option</param>
        /// <param name="num">Block number</param>
        /// <param name="szx">Block size</param>
        /// <param name="m">More flag</param>
        public BlockOption(OptionType type, Int32 num, Int32 szx, Boolean m) : base(type)
        {
            this.IntValue = Encode(num, szx, m);
        }

        /// <summary>
        /// Sets block params.
        /// </summary>
        /// <param name="num">Block number</param>
        /// <param name="szx">Block size</param>
        /// <param name="m">More flag</param>
        public void SetValue(Int32 num, Int32 szx, Boolean m)
        {
            this.IntValue = Encode(num, szx, m);
        }

        /// <summary>
        /// Gets or sets the block number.
        /// </summary>
        public Int32 NUM
        {
            get { return this.IntValue >> 4; }
            set { SetValue(value, SZX, M); }
        }

        /// <summary>
        /// Gets or sets the block size.
        /// </summary>
        public Int32 SZX
        {
            get { return this.IntValue & 0x7; }
            set { SetValue(NUM, value, M); }
        }

        /// <summary>
        /// Gets or sets the more flag.
        /// </summary>
        public Boolean M
        {
            get { return (this.IntValue >> 3 & 0x1) != 0; }
            set { SetValue(NUM, SZX, value); }
        }

        /// <summary>
        /// Gets the decoded block size in bytes (B).
        /// </summary>
        public Int32 Size
        {
            get { return DecodeSZX(this.SZX); }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return String.Format("{0}{1} ({2}B/block [{3}])", NUM, M ? "+" : String.Empty, Size, SZX);
        }

        /// <summary>
        /// Gets the real block size which is 2 ^ (SZX + 4).
        /// </summary>
        /// <param name="szx"></param>
        /// <returns></returns>
        public static Int32 DecodeSZX(Int32 szx)
        {
            return 1 << (szx + 4);
        }

        /// <summary>
        /// Converts a block size into the corresponding SZX.
        /// </summary>
        /// <param name="blockSize"></param>
        /// <returns></returns>
        public static Int32 EncodeSZX(Int32 blockSize)
        {
            if (blockSize < 16)
                return 0;
            if (blockSize > 1024)
                return 6;
            return (Int32)(Math.Log(blockSize) / Math.Log(2)) - 4;
        }

        /// <summary>
        /// Checks whether the given SZX is valid or not.
        /// </summary>
        /// <param name="szx"></param>
        /// <returns></returns>
        public static Boolean ValidSZX(Int32 szx)
        {
            return (szx >= 0 && szx <= 6);
        }

        private static Int32 Encode(Int32 num, Int32 szx, Boolean m)
        {
            Int32 value = 0;
            value |= (szx & 0x7);
            value |= (m ? 1 : 0) << 3;
            value |= num << 4;
            return value;
        }
    }
}
