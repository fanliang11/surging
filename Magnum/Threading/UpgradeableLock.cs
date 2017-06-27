namespace Magnum.Threading
{
    using System;
    using System.Threading;

    public class UpgradeableLock :
    IDisposable
    {
        private readonly ReaderWriterLockSlim _lock;

        public UpgradeableLock()
        {
            _lock = new ReaderWriterLockSlim();
        }

        public bool IsUpgradeableReadLockHeld
        {
            get { return _lock.IsUpgradeableReadLockHeld; }
        }
        public bool IsReadOnlyLockHeld
        {
            get { return _lock.IsReadLockHeld; }
        }
        public bool IsWriteLockHeld
        {
            get { return _lock.IsWriteLockHeld; }
        }


        public EnterUpgradableReader EnterUpgradableRead()
        {
            return new EnterUpgradableReader(_lock);
        }

        public EnterWriterLock EnterWriteLock()
        {
            return new EnterWriterLock(_lock);
        }

        public EnterRead EnterReadOnlyLock()
        {
            return new EnterRead(_lock);
        }

        public void Dispose()
        {
            _lock.Dispose();
        }

        public class EnterRead : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock;

            public EnterRead(ReaderWriterLockSlim lockk)
            {
                _lock = lockk;
                _lock.EnterReadLock();
            }

            public void Dispose()
            {
                _lock.ExitReadLock();
            }
        }
        public class EnterUpgradableReader : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock;

            public EnterUpgradableReader(ReaderWriterLockSlim lockk)
            {
                _lock = lockk;
                _lock.EnterUpgradeableReadLock();
            }

            public EnterWriterLock Upgrade()
            {
                return new EnterWriterLock(_lock);
            }

            public void Dispose()
            {
                _lock.ExitUpgradeableReadLock();
            }
        }

        public class EnterWriterLock : IDisposable
        {
            private readonly ReaderWriterLockSlim _lock;
            public EnterWriterLock(ReaderWriterLockSlim lockk)
            {
                _lock = lockk;
                _lock.EnterWriteLock();
            }

            public void Dispose()
            {
                _lock.ExitWriteLock();
            }
        }
    }
}