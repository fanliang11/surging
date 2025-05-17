/*
 * Copyright 2012 The Netty Project
 *
 * The Netty Project licenses this file to you under the Apache License,
 * version 2.0 (the "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at:
 *
 *   http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations
 * under the License.
 *
 * Copyright (c) 2020 The Dotnetty-Span-Fork Project (cuteant@outlook.com) All rights reserved.
 *
 *   https://github.com/cuteant/dotnetty-span-fork
 *
 * Licensed under the MIT license. See LICENSE file in the project root for full license information.
 */

namespace DotNetty.Transport.Libuv.Native
{
    /// <summary>Reference: http://docs.libuv.org/en/v1.x/errors.html</summary>
    public enum ErrorCode
    {
        /// <summary>UV_E2BIG: argument list too long</summary>
        E2BIG,

        /// <summary>UV_EACCES: permission denied</summary>
        EACCES,

        /// <summary>UV_EADDRINUSE: address already in use</summary>
        EADDRINUSE,

        /// <summary>UV_EADDRNOTAVAIL: address not available</summary>
        EADDRNOTAVAIL,

        /// <summary>UV_EAFNOSUPPORT: address family not supported</summary>
        EAFNOSUPPORT,

        /// <summary>UV_EAGAIN: resource temporarily unavailable</summary>
        EAGAIN,

        /// <summary>UV_EAI_ADDRFAMILY: address family not supported</summary>
        EAI_ADDRFAMILY,

        /// <summary>UV_EAI_AGAIN: temporary failure</summary>
        EAI_AGAIN,

        /// <summary>UV_EAI_BADFLAGS: bad ai_flags value</summary>
        EAI_BADFLAGS,

        /// <summary>UV_EAI_BADHINTS: invalid value for hints</summary>
        EAI_BADHINTS,

        /// <summary>UV_EAI_CANCELED: request canceled</summary>
        EAI_CANCELED,

        /// <summary>UV_EAI_FAIL: permanent failure</summary>
        EAI_FAIL,

        /// <summary>UV_EAI_FAMILY: ai_family not supported</summary>
        EAI_FAMILY,

        /// <summary>UV_EAI_MEMORY: out of memory</summary>
        EAI_MEMORY,

        /// <summary>UV_EAI_NODATA: no address</summary>
        EAI_NODATA,

        /// <summary>UV_EAI_NONAME: unknown node or service</summary>
        EAI_NONAME,

        /// <summary>UV_EAI_OVERFLOW: argument buffer overflow</summary>
        EAI_OVERFLOW,

        /// <summary>UV_EAI_PROTOCOL: resolved protocol is unknown</summary>
        EAI_PROTOCOL,

        /// <summary>UV_EAI_SERVICE: service not available for socket type</summary>
        EAI_SERVICE,

        /// <summary>UV_EAI_SOCKTYPE: socket type not supported</summary>
        EAI_SOCKTYPE,

        /// <summary>UV_EALREADY: connection already in progress</summary>
        EALREADY,

        /// <summary>UV_EBADF: bad file descriptor</summary>
        EBADF,

        /// <summary>UV_EBUSY: resource busy or locked</summary>
        EBUSY,

        /// <summary>UV_ECANCELED: operation canceled</summary>
        ECANCELED,

        /// <summary>UV_ECHARSET: invalid Unicode character</summary>
        ECHARSET,

        /// <summary>UV_ECONNABORTED: software caused connection abort</summary>
        ECONNABORTED,

        /// <summary>UV_ECONNREFUSED: connection refused</summary>
        ECONNREFUSED,

        /// <summary>UV_ECONNRESET: connection reset by peer</summary>
        ECONNRESET,

        /// <summary>UV_EDESTADDRREQ: destination address required</summary>
        EDESTADDRREQ,

        /// <summary>UV_EEXIST: file already exists</summary>
        EEXIST,

        /// <summary>UV_EFAULT: bad address in system call argument</summary>
        EFAULT,

        /// <summary>UV_EFBIG: file too large</summary>
        EFBIG,

        /// <summary>UV_EHOSTUNREACH: host is unreachable</summary>
        EHOSTUNREACH,

        /// <summary>UV_EINTR: interrupted system call</summary>
        EINTR,

        /// <summary>UV_EINVAL: invalid argument</summary>
        EINVAL,

        /// <summary>UV_EIO: i/o error</summary>
        EIO,

        /// <summary>UV_EISCONN: socket is already connected</summary>
        EISCONN,

        /// <summary>UV_EISDIR: illegal operation on a directory</summary>
        EISDIR,

        /// <summary>UV_ELOOP: too many symbolic links encountered</summary>
        ELOOP,

        /// <summary>UV_EMFILE: too many open files</summary>
        EMFILE,

        /// <summary>UV_EMSGSIZE: message too long</summary>
        EMSGSIZE,

        /// <summary>UV_ENAMETOOLONG: name too long</summary>
        ENAMETOOLONG,

        /// <summary>UV_ENETDOWN: network is down</summary>
        ENETDOWN,

        /// <summary>UV_ENETUNREACH: network is unreachable</summary>
        ENETUNREACH,

        /// <summary>UV_ENFILE: file table overflow</summary>
        ENFILE,

        /// <summary>UV_ENOBUFS: no buffer space available</summary>
        ENOBUFS,

        /// <summary>UV_ENODEV: no such device</summary>
        ENODEV,

        /// <summary>UV_ENOENT: no such file or directory</summary>
        ENOENT,

        /// <summary>UV_ENOMEM: not enough memory</summary>
        ENOMEM,

        /// <summary>UV_ENONET: machine is not on the network</summary>
        ENONET,

        /// <summary>UV_ENOPROTOOPT: protocol not available</summary>
        ENOPROTOOPT,

        /// <summary>UV_ENOSPC: no space left on device</summary>
        ENOSPC,

        /// <summary>UV_ENOSYS: function not implemented</summary>
        ENOSYS,

        /// <summary>UV_ENOTCONN: socket is not connected</summary>
        ENOTCONN,

        /// <summary>UV_ENOTDIR: not a directory</summary>
        ENOTDIR,

        /// <summary>UV_ENOTEMPTY: directory not empty</summary>
        ENOTEMPTY,

        /// <summary>UV_ENOTSOCK: socket operation on non-socket</summary>
        ENOTSOCK,

        /// <summary>UV_ENOTSUP: operation not supported on socket</summary>
        ENOTSUP,

        /// <summary>UV_EPERM: operation not permitted</summary>
        EPERM,

        /// <summary>UV_EPIPE: broken pipe</summary>
        EPIPE,

        /// <summary>UV_EPROTO: protocol error</summary>
        EPROTO,

        /// <summary>UV_EPROTONOSUPPORT: protocol not supported</summary>
        EPROTONOSUPPORT,

        /// <summary>UV_EPROTOTYPE: protocol wrong type for socket</summary>
        EPROTOTYPE,

        /// <summary>UV_ERANGE: result too large</summary>
        ERANGE,

        /// <summary>UV_EROFS: read-only file system</summary>
        EROFS,

        /// <summary>UV_ESHUTDOWN: cannot send after transport endpoint shutdown</summary>
        ESHUTDOWN,

        /// <summary>UV_ESPIPE: invalid seek</summary>
        ESPIPE,

        /// <summary>UV_ESRCH: no such process</summary>
        ESRCH,

        /// <summary>UV_ETIMEDOUT: connection timed out</summary>
        ETIMEDOUT,

        /// <summary>UV_ETXTBSY: text file is busy</summary>
        ETXTBSY,

        /// <summary>UV_EXDEV: cross-device link not permitted</summary>
        EXDEV,

        /// <summary>UV_UNKNOWN: unknown error</summary>
        UNKNOWN,

        /// <summary>UV_EOF: end of file</summary>
        EOF,

        /// <summary>UV_ENXIO: no such device or address</summary>
        ENXIO,

        /// <summary>UV_EMLINK: too many links</summary>
        EMLINK,
    }
}