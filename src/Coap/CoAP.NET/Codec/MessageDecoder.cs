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

namespace CoAP.Codec
{
    /// <summary>
    /// Base class for message decoders.
    /// </summary>
    public abstract class MessageDecoder : IMessageDecoder
    {
        /// <summary>
        /// the bytes reader
        /// </summary>
        protected DatagramReader m_reader;
        /// <summary>
        /// the version of the decoding message
        /// </summary>
        protected Int32 m_version;
        /// <summary>
        /// the type of the decoding message
        /// </summary>
        protected MessageType m_type;
        /// <summary>
        /// the length of token
        /// </summary>
        protected Int32 m_tokenLength;
        /// <summary>
        /// the code of the decoding message
        /// </summary>
        protected Int32 m_code;
        /// <summary>
        /// the id of the decoding message
        /// </summary>
        protected Int32 m_id;

        /// <summary>
        /// Instantiates.
        /// </summary>
        /// <param name="data">the bytes array to decode</param>
        public MessageDecoder(Byte[] data)
        {
            m_reader = new DatagramReader(data);
        }

        /// <summary>
        /// Reads protocol headers.
        /// </summary>
        protected abstract void ReadProtocol();

        /// <inheritdoc/>
        public abstract Boolean IsWellFormed { get; }

        /// <inheritdoc/>
        public Boolean IsReply
        {
            get { return m_type == MessageType.ACK || m_type == MessageType.RST; }
        }

        /// <inheritdoc/>
        public virtual Boolean IsRequest
        {
            get
            {
                return m_code >= CoapConstants.RequestCodeLowerBound &&
                    m_code <= CoapConstants.RequestCodeUpperBound;
            }
        }

        /// <inheritdoc/>
        public virtual Boolean IsResponse
        {
            get
            {
                return m_code >= CoapConstants.ResponseCodeLowerBound &&
                  m_code <= CoapConstants.ResponseCodeUpperBound;
            }
        }

        /// <inheritdoc/>
        public Boolean IsEmpty
        {
            get { return m_code == Code.Empty; }
        }

        /// <inheritdoc/>
        public Int32 Version
        {
            get { return m_version; }
        }

        /// <inheritdoc/>
        public Int32 ID
        {
            get { return m_id; }
        }

        /// <inheritdoc/>
        public Request DecodeRequest()
        {
            System.Diagnostics.Debug.Assert(IsRequest);
            Request request = new Request((Method)m_code);
            request.Type = m_type;
            request.ID = m_id;
            ParseMessage(request);
            return request;
        }

        /// <inheritdoc/>
        public Response DecodeResponse()
        {
            System.Diagnostics.Debug.Assert(IsResponse);
            Response response = new Response((StatusCode)m_code);
            response.Type = m_type;
            response.ID = m_id;
            ParseMessage(response);
            return response;
        }

        /// <inheritdoc/>
        public EmptyMessage DecodeEmptyMessage()
        {
            System.Diagnostics.Debug.Assert(!IsRequest && !IsResponse);
            EmptyMessage message = new EmptyMessage(m_type);
            message.Type = m_type;
            message.ID = m_id;
            ParseMessage(message);
            return message;
        }

        /// <inheritdoc/>
        public Message Decode()
        {
            if (IsRequest)
                return DecodeRequest();
            else if (IsResponse)
                return DecodeResponse();
            else if (IsEmpty)
                return DecodeEmptyMessage();
            else
                return null;
        }

        /// <summary>
        /// Parses the rest data other than protocol headers into the given message.
        /// </summary>
        /// <param name="message"></param>
        protected abstract void ParseMessage(Message message);
    }
}
