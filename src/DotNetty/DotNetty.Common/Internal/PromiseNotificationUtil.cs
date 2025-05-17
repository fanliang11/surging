namespace DotNetty.Common.Internal
{
    using System;
    using DotNetty.Common.Concurrency;
    using DotNetty.Common.Internal.Logging;

    /// <summary>
    /// Internal utilities to notify <see cref="IPromise"/>s.
    /// </summary>
    public static class PromiseNotificationUtil
    {
        /// <summary>
        /// Try to cancel the <see cref="IPromise"/> and log if <paramref name="logger"/> is not <c>null</c> in case this fails.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="logger"></param>
        public static void TryCancel(IPromise p, IInternalLogger logger)
        {
            if (!p.TrySetCanceled() && logger is object)
            {
                logger.FailedToMarkAPromiseAsCancel(p);
            }
        }

        /// <summary>
        /// Try to mark the <see cref="IPromise"/> as success and log if <paramref name="logger"/> is not <c>null</c> in case this fails.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="logger"></param>
        public static void TrySuccess(IPromise p, IInternalLogger logger)
        {
            if (!p.TryComplete() && logger is object)
            {
                logger.FailedToMarkAPromiseAsSuccess(p);
            }
        }

        /// <summary>
        /// Try to mark the <see cref="IPromise"/> as failure and log if <paramref name="logger"/> is not <c>null</c> in case this fails.
        /// </summary>
        /// <param name="p"></param>
        /// <param name="cause"></param>
        /// <param name="logger"></param>
        public static void TryFailure(IPromise p, Exception cause, IInternalLogger logger)
        {
            if (!p.TrySetException(cause) && logger is object)
            {
                logger.FailedToMarkAPromiseAsFailure(p, cause);
            }
        }
    }
}
