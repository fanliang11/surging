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
using System.Collections.Generic;

namespace CoAP.Stack
{
    /// <summary>
    /// Represents the status of a blockwise transfer of a request or a response.
    /// </summary>
    public class BlockwiseStatus
    {
        public const Int32 NoObserve = -1;

        private Int32 _currentNUM;
        private Int32 _currentSZX;
        private Boolean _randomAccess;
        private readonly Int32 _contentFormat;
        private Boolean _complete;
        private Int32 _observe = NoObserve;
        private List<Byte[]> _blocks = new List<Byte[]>();

        /// <summary>
        /// Instantiates a new blockwise status.
        /// </summary>
        public BlockwiseStatus(Int32 contentFormat)
        {
            _contentFormat = contentFormat;
        }

        /// <summary>
        /// Instantiates a new blockwise status.
        /// </summary>
        public BlockwiseStatus(Int32 contentFormat, Int32 num, Int32 szx)
        {
            _contentFormat = contentFormat;
            _currentNUM = num;
            _currentSZX = szx;
        }

        /// <summary>
        /// Gets or sets the current num.
        /// </summary>
        public Int32 CurrentNUM
        {
            get { return _currentNUM; }
            set { _currentNUM = value; }
        }

        /// <summary>
        /// Gets or sets the current szx.
        /// </summary>
        public Int32 CurrentSZX
        {
            get { return _currentSZX; }
            set { _currentSZX = value; }
        }

        /// <summary>
        /// Gets or sets if this status is for random access.
        /// </summary>
        public Boolean IsRandomAccess
        {
            get { return _randomAccess; }
            set { _randomAccess = value; }
        }

        /// <summary>
        /// Gets the initial Content-Format, which must stay the same for the whole transfer.
        /// </summary>
        public Int32 ContentFormat
        {
            get { return _contentFormat; }
        }

        /// <summary>
        /// Gets or sets a value indicating if this is complete.
        /// </summary>
        public Boolean Complete
        {
            get { return _complete; }
            set { _complete = value; }
        }

        public Int32 Observe
        {
            get { return _observe; }
            set { _observe = value; }
        }

        /// <summary>
        /// Gets the number of blocks.
        /// </summary>
        public Int32 BlockCount
        {
            get { return _blocks.Count; }
        }

        /// <summary>
        /// Gets all blocks.
        /// </summary>
        public IEnumerable<Byte[]> Blocks
        {
            get { return _blocks; }
        }

        /// <summary>
        /// Adds the specified block to the current list of blocks.
        /// </summary>
        public void AddBlock(Byte[] block)
        {
            if (block != null)
                _blocks.Add(block);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return String.Format("[CurrentNum={0}, CurrentSzx={1}, Complete={2}, RandomAccess={3}]",
                _currentNUM, _currentSZX, _complete, _randomAccess);
        }
    }
}
