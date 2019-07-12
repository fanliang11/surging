using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Surging.Core.CPlatform.Utilities
{
    #region 枚举

    /// <summary>
    /// Defines the ContinuationOptions
    /// </summary>
    public enum ContinuationOptions
    {
        /// <summary>
        /// Defines the None
        /// </summary>
        None,

        /// <summary>
        /// Defines the ForceDefaultTaskScheduler
        /// </summary>
        ForceDefaultTaskScheduler
    }

    #endregion 枚举

    #region 接口

    /// <summary>
    /// Defines the <see cref="IStrongBox{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal interface IStrongBox<T>
    {
        #region 属性

        /// <summary>
        /// Gets or sets a value indicating whether RunContinuationsAsynchronously
        /// </summary>
        bool RunContinuationsAsynchronously { get; set; }

        /// <summary>
        /// Gets the Value
        /// </summary>
        ref T Value { get; }

        #endregion 属性
    }

    #endregion 接口

    /// <summary>
    /// Defines the <see cref="ManualResetValueTaskSourceLogic{TResult}" />
    /// </summary>
    /// <typeparam name="TResult"></typeparam>
    internal struct ManualResetValueTaskSourceLogic<TResult>
    {
        #region 字段

        /// <summary>
        /// Defines the s_sentinel
        /// </summary>
        private static readonly Action<object> s_sentinel = s => throw new InvalidOperationException();

        /// <summary>
        /// Defines the _options
        /// </summary>
        private readonly ContinuationOptions _options;

        /// <summary>
        /// Defines the _parent
        /// </summary>
        private readonly IStrongBox<ManualResetValueTaskSourceLogic<TResult>> _parent;

        /// <summary>
        /// Defines the _capturedContext
        /// </summary>
        private object _capturedContext;

        /// <summary>
        /// Defines the _completed
        /// </summary>
        private bool _completed;

        /// <summary>
        /// Defines the _continuation
        /// </summary>
        private Action<object> _continuation;

        /// <summary>
        /// Defines the _continuationState
        /// </summary>
        private object _continuationState;

        /// <summary>
        /// Defines the _error
        /// </summary>
        private ExceptionDispatchInfo _error;

        /// <summary>
        /// Defines the _executionContext
        /// </summary>
        private ExecutionContext _executionContext;

        /// <summary>
        /// Defines the _registration
        /// </summary>
        private CancellationTokenRegistration? _registration;

        /// <summary>
        /// Defines the _result
        /// </summary>
        private TResult _result;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref=""/> class.
        /// </summary>
        /// <param name="parent">The parent<see cref="IStrongBox{ManualResetValueTaskSourceLogic{TResult}}"/></param>
        /// <param name="options">The options<see cref="ContinuationOptions"/></param>
        public ManualResetValueTaskSourceLogic(IStrongBox<ManualResetValueTaskSourceLogic<TResult>> parent, ContinuationOptions options)
        {
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
            _options = options;
            _continuation = null;
            _continuationState = null;
            _capturedContext = null;
            _executionContext = null;
            _completed = false;
            _result = default(TResult);
            _error = null;
            Version = 0;
            _registration = null;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets a value indicating whether Completed
        /// </summary>
        public bool Completed => _completed;

        /// <summary>
        /// Gets the Version
        /// </summary>
        public short Version { get; private set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The AwaitValue
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source">The source<see cref="IValueTaskSource{T}"/></param>
        /// <param name="registration">The registration<see cref="CancellationTokenRegistration?"/></param>
        /// <returns>The <see cref="ValueTask{T}"/></returns>
        public ValueTask<T> AwaitValue<T>(IValueTaskSource<T> source, CancellationTokenRegistration? registration)
        {
            _registration = registration;
            return new ValueTask<T>(source, Version);
        }

        /// <summary>
        /// The AwaitVoid
        /// </summary>
        /// <param name="source">The source<see cref="IValueTaskSource"/></param>
        /// <param name="registration">The registration<see cref="CancellationTokenRegistration?"/></param>
        /// <returns>The <see cref="ValueTask"/></returns>
        public ValueTask AwaitVoid(IValueTaskSource source, CancellationTokenRegistration? registration)
        {
            _registration = registration;
            return new ValueTask(source, Version);
        }

        /// <summary>
        /// The GetResult
        /// </summary>
        /// <param name="token">The token<see cref="short"/></param>
        /// <returns>The <see cref="TResult"/></returns>
        public TResult GetResult(short token)
        {
            ValidateToken(token);

            if (!_completed)
            {
                throw new InvalidOperationException();
            }

            TResult result = _result;
            ExceptionDispatchInfo error = _error;
            Reset();

            error?.Throw();
            return result;
        }

        /// <summary>
        /// The GetStatus
        /// </summary>
        /// <param name="token">The token<see cref="short"/></param>
        /// <returns>The <see cref="ValueTaskSourceStatus"/></returns>
        public ValueTaskSourceStatus GetStatus(short token)
        {
            ValidateToken(token);

            return
                !_completed ? ValueTaskSourceStatus.Pending :
                _error == null ? ValueTaskSourceStatus.Succeeded :
                _error.SourceException is OperationCanceledException ? ValueTaskSourceStatus.Canceled :
                ValueTaskSourceStatus.Faulted;
        }

        /// <summary>
        /// The OnCompleted
        /// </summary>
        /// <param name="continuation">The continuation<see cref="Action{object}"/></param>
        /// <param name="state">The state<see cref="object"/></param>
        /// <param name="token">The token<see cref="short"/></param>
        /// <param name="flags">The flags<see cref="ValueTaskSourceOnCompletedFlags"/></param>
        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags)
        {
            if (continuation == null)
            {
                throw new ArgumentNullException(nameof(continuation));
            }

            ValidateToken(token);

            if ((flags & ValueTaskSourceOnCompletedFlags.FlowExecutionContext) != 0)
            {
                _executionContext = ExecutionContext.Capture();
            }

            if ((flags & ValueTaskSourceOnCompletedFlags.UseSchedulingContext) != 0)
            {
                SynchronizationContext sc = SynchronizationContext.Current;
                if (sc != null && sc.GetType() != typeof(SynchronizationContext))
                {
                    _capturedContext = sc;
                }
                else
                {
                    TaskScheduler ts = TaskScheduler.Current;
                    if (ts != TaskScheduler.Default)
                    {
                        _capturedContext = ts;
                    }
                }
            }

            _continuationState = state;
            if (Interlocked.CompareExchange(ref _continuation, continuation, null) != null)
            {
                _executionContext = null;

                object cc = _capturedContext;
                _capturedContext = null;

                switch (cc)
                {
                    case null:
                        Task.Factory.StartNew(continuation, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default);
                        break;

                    case SynchronizationContext sc:
                        sc.Post(s =>
                        {
                            var tuple = (Tuple<Action<object>, object>)s;
                            tuple.Item1(tuple.Item2);
                        }, Tuple.Create(continuation, state));
                        break;

                    case TaskScheduler ts:
                        Task.Factory.StartNew(continuation, state, CancellationToken.None, TaskCreationOptions.DenyChildAttach, ts);
                        break;
                }
            }
        }

        /// <summary>
        /// The Reset
        /// </summary>
        public void Reset()
        {
            Version++;

            _registration?.Dispose();

            _completed = false;
            _continuation = null;
            _continuationState = null;
            _result = default(TResult);
            _error = null;
            _executionContext = null;
            _capturedContext = null;
            _registration = null;
        }

        /// <summary>
        /// The SetException
        /// </summary>
        /// <param name="error">The error<see cref="Exception"/></param>
        public void SetException(Exception error)
        {
            _error = ExceptionDispatchInfo.Capture(error);
            SignalCompletion();
        }

        /// <summary>
        /// The SetResult
        /// </summary>
        /// <param name="result">The result<see cref="TResult"/></param>
        public void SetResult(TResult result)
        {
            _result = result;
            SignalCompletion();
        }

        /// <summary>
        /// The InvokeContinuation
        /// </summary>
        private void InvokeContinuation()
        {
            object cc = _capturedContext;
            _capturedContext = null;

            if (_options == ContinuationOptions.ForceDefaultTaskScheduler)
            {
                cc = TaskScheduler.Default;
            }

            switch (cc)
            {
                case null:
                    if (_parent.RunContinuationsAsynchronously)
                    {
                        var c = _continuation;
                        if (_executionContext != null)
                        {
                            ThreadPool.QueueUserWorkItem(s => c(s), _continuationState);
                        }
                        else
                        {
                            ThreadPool.UnsafeQueueUserWorkItem(s => c(s), _continuationState);
                        }
                    }
                    else
                    {
                        _continuation(_continuationState);
                    }
                    break;

                case SynchronizationContext sc:
                    sc.Post(s =>
                    {
                        ref ManualResetValueTaskSourceLogic<TResult> logicRef = ref ((IStrongBox<ManualResetValueTaskSourceLogic<TResult>>)s).Value;
                        logicRef._continuation(logicRef._continuationState);
                    }, _parent ?? throw new InvalidOperationException());
                    break;

                case TaskScheduler ts:
                    Task.Factory.StartNew(_continuation, _continuationState, CancellationToken.None, TaskCreationOptions.DenyChildAttach, ts);
                    break;
            }
        }

        /// <summary>
        /// The SignalCompletion
        /// </summary>
        private void SignalCompletion()
        {
            if (_completed)
            {
                throw new InvalidOperationException("Double completion of completion source is prohibited");
            }

            _completed = true;

            if (Interlocked.CompareExchange(ref _continuation, s_sentinel, null) != null)
            {
                if (_executionContext != null)
                {
                    ExecutionContext.Run(
                        _executionContext,
                        s => ((IStrongBox<ManualResetValueTaskSourceLogic<TResult>>)s).Value.InvokeContinuation(),
                        _parent ?? throw new InvalidOperationException());
                }
                else
                {
                    InvokeContinuation();
                }
            }
        }

        /// <summary>
        /// The ValidateToken
        /// </summary>
        /// <param name="token">The token<see cref="short"/></param>
        private void ValidateToken(short token)
        {
            if (token != Version)
            {
                throw new InvalidOperationException();
            }
        }

        #endregion 方法
    }

    /// <summary>
    /// Defines the <see cref="ManualResetValueTaskSource{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ManualResetValueTaskSource<T> : IStrongBox<ManualResetValueTaskSourceLogic<T>>, IValueTaskSource<T>, IValueTaskSource
    {
        #region 字段

        /// <summary>
        /// Defines the _cancellationCallback
        /// </summary>
        private readonly Action _cancellationCallback;

        /// <summary>
        /// Defines the _logic
        /// </summary>
        private ManualResetValueTaskSourceLogic<T> _logic;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ManualResetValueTaskSource{T}"/> class.
        /// </summary>
        /// <param name="options">The options<see cref="ContinuationOptions"/></param>
        public ManualResetValueTaskSource(ContinuationOptions options = ContinuationOptions.None)
        {
            _logic = new ManualResetValueTaskSourceLogic<T>(this, options);
            _cancellationCallback = SetCanceled;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets a value indicating whether RunContinuationsAsynchronously
        /// </summary>
        public bool RunContinuationsAsynchronously { get; set; } = true;

        /// <summary>
        /// Gets the Version
        /// </summary>
        public short Version => _logic.Version;

        /// <summary>
        /// Gets the Value
        /// </summary>
        ref ManualResetValueTaskSourceLogic<T> IStrongBox<ManualResetValueTaskSourceLogic<T>>.Value => ref _logic;

        #endregion 属性

        #region 方法

        /// <summary>
        /// The AwaitValue
        /// </summary>
        /// <param name="cancellation">The cancellation<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="ValueTask{T}"/></returns>
        public ValueTask<T> AwaitValue(CancellationToken cancellation)
        {
            CancellationTokenRegistration? registration = cancellation == CancellationToken.None
                ? (CancellationTokenRegistration?)null
                : cancellation.Register(_cancellationCallback);
            return _logic.AwaitValue(this, registration);
        }

        /// <summary>
        /// The AwaitVoid
        /// </summary>
        /// <param name="cancellation">The cancellation<see cref="CancellationToken"/></param>
        /// <returns>The <see cref="ValueTask"/></returns>
        public ValueTask AwaitVoid(CancellationToken cancellation)
        {
            CancellationTokenRegistration? registration = cancellation == CancellationToken.None
                ? (CancellationTokenRegistration?)null
                : cancellation.Register(_cancellationCallback);
            return _logic.AwaitVoid(this, registration);
        }

        /// <summary>
        /// The GetResult
        /// </summary>
        /// <param name="token">The token<see cref="short"/></param>
        /// <returns>The <see cref="T"/></returns>
        public T GetResult(short token) => _logic.GetResult(token);

        /// <summary>
        /// The GetStatus
        /// </summary>
        /// <param name="token">The token<see cref="short"/></param>
        /// <returns>The <see cref="ValueTaskSourceStatus"/></returns>
        public ValueTaskSourceStatus GetStatus(short token) => _logic.GetStatus(token);

        /// <summary>
        /// The OnCompleted
        /// </summary>
        /// <param name="continuation">The continuation<see cref="Action{object}"/></param>
        /// <param name="state">The state<see cref="object"/></param>
        /// <param name="token">The token<see cref="short"/></param>
        /// <param name="flags">The flags<see cref="ValueTaskSourceOnCompletedFlags"/></param>
        public void OnCompleted(Action<object> continuation, object state, short token, ValueTaskSourceOnCompletedFlags flags) => _logic.OnCompleted(continuation, state, token, flags);

        /// <summary>
        /// The Reset
        /// </summary>
        public void Reset() => _logic.Reset();

        /// <summary>
        /// The SetCanceled
        /// </summary>
        public void SetCanceled() => SetException(new TaskCanceledException());

        /// <summary>
        /// The SetException
        /// </summary>
        /// <param name="error">The error<see cref="Exception"/></param>
        public void SetException(Exception error)
        {
            if (Monitor.TryEnter(_cancellationCallback))
            {
                if (_logic.Completed)
                {
                    Monitor.Exit(_cancellationCallback);
                    return;
                }

                _logic.SetException(error);
                Monitor.Exit(_cancellationCallback);
            }
        }

        /// <summary>
        /// The SetResult
        /// </summary>
        /// <param name="result">The result<see cref="T"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public bool SetResult(T result)
        {
            lock (_cancellationCallback)
            {
                if (_logic.Completed)
                {
                    return false;
                }

                _logic.SetResult(result);
                return true;
            }
        }

        /// <summary>
        /// The GetResult
        /// </summary>
        /// <param name="token">The token<see cref="short"/></param>
        void IValueTaskSource.GetResult(short token) => _logic.GetResult(token);

        #endregion 方法
    }
}