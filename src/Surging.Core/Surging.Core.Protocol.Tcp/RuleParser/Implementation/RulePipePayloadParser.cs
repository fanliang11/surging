using DotNetty.Buffers;
using Jint;
using Surging.Core.Protocol.Tcp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Protocol.Tcp.RuleParser.Implementation
{
    public class RulePipePayloadParser : IRulePayloadParser
    {
        private readonly List<IByteBuffer> _result = new List<IByteBuffer>();
        private ISubject<IByteBuffer> _bufferSubject = new ReplaySubject<IByteBuffer>();
        private readonly List<Action<IByteBuffer>> _pipe = new List<Action<IByteBuffer>>();
        private readonly List<int> _fixedRecordLength = new List<int>();
        private readonly AtomicInteger _currentPipe = new AtomicInteger();
        private readonly AtomicInteger _currentFixedRecordLength = new AtomicInteger();
        private  Func<IByteBuffer, IByteBuffer> _directMapper;
        public RulePipePayloadParser Result(IByteBuffer buffer)
        {
            _result.Add(buffer);
            return this;
        }

        public RulePipePayloadParser Result(string buffer)
        {
            return Result(Unpooled.CopiedBuffer(buffer, Encoding.UTF8));
        }

        public RulePipePayloadParser Result(string buffer,string encodeName)
        {
            return Result(Unpooled.CopiedBuffer(buffer, Encoding.GetEncoding(encodeName)));
        }

        public RulePipePayloadParser Result(byte[] buffer)
        {
            return Result(Unpooled.CopiedBuffer(buffer));
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
            int i = _currentPipe.Increment()-1;
            if (i < _pipe.Count)
            {
                return _pipe[i];
            }
            _currentPipe.Value = 0;
            return _pipe[0];
        }

        public int GetNextFixedRecordLength()
        {
            int i = _currentFixedRecordLength.Increment()-1;
            if (i < _fixedRecordLength.Count)
            {
                return _fixedRecordLength[i];
            }
            _currentFixedRecordLength.Value = 0;
            return _fixedRecordLength[0];
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


        public RulePipePayloadParser Complete()
        {
            _currentPipe.Value = 0;
            _currentFixedRecordLength.Value = 0;
            if (_result.Any())
            {
                var buffer = Unpooled.Buffer();
                
                foreach (var buf in _result)
                { 

                     buf.ReadBytes(buffer, buf.ReadableBytes);
                 
                    
                }
                _result.Clear();
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
                catch(Exception ex)
                {
                    throw ex;
                }
            });
            return this;
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
            _currentFixedRecordLength.Value=0;
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
