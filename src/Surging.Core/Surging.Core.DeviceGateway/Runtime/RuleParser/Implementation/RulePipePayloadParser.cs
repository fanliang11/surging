using DotNetty.Buffers;
using DotNetty.Common;
using Jint;
using Surging.Core.DeviceGateway.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.DeviceGateway.Runtime.RuleParser.Implementation
{
    public class RulePipePayloadParser : IRulePayloadParser
    {
        private readonly List<IByteBuffer> _result = new List<IByteBuffer>();
        private readonly List<IByteBuffer> _delimited = new List<IByteBuffer>();
        private bool discardingTooLongFrame = true;
        private ISubject<IByteBuffer> _bufferSubject = new ReplaySubject<IByteBuffer>();
        private readonly List<Action<IByteBuffer>> _pipe = new List<Action<IByteBuffer>>();
        private readonly List<int> _fixedRecordLength = new List<int>();
        private readonly AtomicInteger _currentPipe = new AtomicInteger();
        private readonly AtomicInteger _currentFixedRecordLength = new AtomicInteger();
        private Func<IByteBuffer, IByteBuffer> _directMapper;
        private int _bufLen = 0;

        public RulePipePayloadParser Result(IByteBuffer buffer)
        {
            _result.Add(buffer);
            _bufLen += buffer.ReadableBytes;
            //_pipe.Add(buffer =>
            //{
            //    IReferenceCounted referenceCounted = null;
            //    do
            //    {
            //        referenceCounted = NewDelimited(buffer);
            //    } while (referenceCounted != null);
            //});
            return this;
        }

        public RulePipePayloadParser Delimited(IByteBuffer buffer)
        {
            _delimited.Add(buffer);
            return this;
        }

        public IReferenceCounted NewDelimited(IByteBuffer buffer)
        {
            int num = int.MaxValue;
            IByteBuffer byteBuffer = null;
            IByteBuffer[] array = _delimited.ToArray();
            foreach (IByteBuffer byteBuffer2 in array)
            {
                int num2 = IndexOf(buffer, byteBuffer2);
                if (num2 >= 0 && num2 < num)
                {
                    num = num2;
                    byteBuffer = byteBuffer2;
                }
            }

            if (byteBuffer != null)
            {
                int capacity = byteBuffer.Capacity;
                if (discardingTooLongFrame)
                {
                    discardingTooLongFrame = false;
                    buffer.SkipBytes(num + capacity);


                    return null;
                }


                IByteBuffer byteBuffer3;

                byteBuffer3 = buffer.ReadSlice(num);
                buffer.SkipBytes(capacity);

                return byteBuffer3.Retain();
            }
            return null;
        }

        public RulePipePayloadParser Delimited(string delim)
        {
            return Result(Unpooled.CopiedBuffer(delim, Encoding.UTF8));
        }

        public RulePipePayloadParser Delimited(byte[] delim)
        {
            return Result(Unpooled.CopiedBuffer(delim));
        }

        public RulePipePayloadParser Result(string buffer)
        {
            return Result(Unpooled.CopiedBuffer(buffer, Encoding.UTF8));
        }

        public RulePipePayloadParser Result(byte[] buffer)
        {
            return Result(Unpooled.CopiedBuffer(buffer));
        }

        public RulePipePayloadParser Result(string buffer, string encodeName)
        {
            return Result(Unpooled.CopiedBuffer(buffer, Encoding.GetEncoding(encodeName)));
        }

        public RulePipePayloadParser Fixed(int length)
        {
            _fixedRecordLength.Add(length);
            return this;
        }

        public void Handle(IByteBuffer buffer)
        {
            if (_directMapper == null)
            {
                return;
            }
            var buf = _directMapper.Invoke(buffer);
            if (null != buf)
            {
                _bufferSubject.OnNext(buf);
            }
        }

        public Action<IByteBuffer> GetNextHandler()
        {
            int i = _currentPipe.Increment() - 1;
            if (i < _pipe.Count)
            {
                return _pipe[i];
            }
            _currentPipe.Value = 0;
            return _pipe[0];
        }

        public int GetNextFixedRecordLength()
        {
            int i = _currentFixedRecordLength.Increment() - 1;
            if (i < _fixedRecordLength.Count)
            {
                return _fixedRecordLength[i];
            }
            _currentFixedRecordLength.Value = 0;
            return default;
        }

        public void Build(IByteBuffer buffer)
        {
            if (_pipe.Any())
            {
                foreach (var pipe in _pipe)
                {
                    try
                    {
                        pipe.Invoke(buffer);
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }
            }
        }

        private static int IndexOf(IByteBuffer haystack, IByteBuffer needle)
        {
            for (int i = haystack.ReaderIndex; i < haystack.WriterIndex; i++)
            {
                int num = i;
                int j;
                for (j = 0; j < needle.Capacity && haystack.GetByte(num) == needle.GetByte(j); j++)
                {
                    num++;
                    if (num == haystack.WriterIndex && j != needle.Capacity - 1)
                    {
                        return -1;
                    }
                }

                if (j == needle.Capacity)
                {
                    return i - haystack.ReaderIndex;
                }
            }

            return -1;
        }
        public RulePipePayloadParser Complete()
        {
            _currentPipe.Value = 0;
            _currentFixedRecordLength.Value = 0;
            if (_result.Any())
            {
                var buffer = Unpooled.Buffer(_bufLen);

                foreach (var buf in _result)
                {
                    var index = buf.ReaderIndex;
                    buf.ReadBytes(buffer, buf.ReadableBytes);
                    buf.SetReaderIndex(index);
                }
                _bufferSubject.OnNext(buffer);
            }
            _bufferSubject.OnCompleted();
            return this;

        }

        public RulePipePayloadParser Handler(Action<IByteBuffer> handler)
        {
            _pipe.Add(handler);
            return this;
        }

        public RulePipePayloadParser Handler(string script)
        {
            var propertyName = "script";
            script = $"var {propertyName} ={script}";
            var engine = new Engine()
       .SetValue("parser", this).SetValue("BytesUtils", new BytesUtils()).Execute(script);
            _pipe.Add(buffer =>
            {
                try
                {
                    engine.Invoke(propertyName, buffer);
                }
                catch (Exception)
                {
                    throw;
                }
            });
            return this;
        }

        public List<IByteBuffer> GetResult()
        { 
            return _result; 
        }

        public ISubject<IByteBuffer> HandlePayload()
        {
            return _bufferSubject;
        }

        public void Reset()
        {
            _result.Clear();
            Complete();
        }

        public void Close()
        {
            _bufferSubject.OnCompleted();
            _currentFixedRecordLength.Value = 0;
            _currentPipe.Value = 0;
            _result.Clear();
            _bufferSubject = new ReplaySubject<IByteBuffer>();
        }

        public RulePipePayloadParser Direct(Func<IByteBuffer, IByteBuffer> mapper)
        {
            _directMapper = mapper;
            return this;
        }
    }
}
