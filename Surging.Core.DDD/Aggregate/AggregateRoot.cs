using Surging.Core.DDD.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;


namespace Surging.Core.DDD
{
    /// <summary>Represents an abstract base aggregate root.
    /// </summary>
    /// <typeparam name="TAggregateRootId"></typeparam>
    [Serializable]
    public abstract class AggregateRoot<TAggregateRootId> : DomainObject
    {
        private static readonly IList<IDomainEvent> _emptyEvents = new List<IDomainEvent>();
        private static IAggregateRootInternalHandlerProvider _eventHandlerProvider;
        private int _version;
        private Queue<IDomainEvent> _uncommittedEvents;
        protected TAggregateRootId _id;

        /// <summary>Gets or sets the unique identifier of the aggregate root.
        /// </summary>
        public TAggregateRootId Id
        {
            get { return _id; }
            protected set { _id = value; }
        }

        /// <summary>Default constructor.
        /// </summary>
        protected AggregateRoot()
        {
            _uncommittedEvents = new Queue<IDomainEvent>();
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        protected AggregateRoot(TAggregateRootId id) : this()
        {
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }
            _id = id;
        }
        /// <summary>Parameterized constructor.
        /// </summary>
        protected AggregateRoot(TAggregateRootId id, int version) : this(id)
        {
            if (version < 0)
            {
                throw new ArgumentException(string.Format("Version cannot small than zero, aggregateRootId: {0}, version: {1}", id, version));
            }
            _version = version;
        }

        /// <summary>Act the current aggregate as the given type of role.
        /// <remarks>
        /// Rhe current aggregate must implement the role interface, otherwise this method will throw exception.
        /// </remarks>
        /// </summary>
        /// <typeparam name="TRole">The role interface type.</typeparam>
        /// <returns>Returns the role instance which is acted by the current aggregate.</returns>
        public TRole ActAs<TRole>() where TRole : class
        {
            if (!typeof(TRole).IsInterface)
            {
                throw new Exception(string.Format("'{0}' is not an interface type.", typeof(TRole).FullName));
            }

            var actor = this as TRole;

            if (actor == null)
            {
                throw new Exception(string.Format("'{0}' cannot act as role '{1}'.", GetType().FullName, typeof(TRole).FullName));
            }

            return actor;
        }
        /// <summary>Apply a domain event to the current aggregate root.
        /// <remarks>
        /// ENode will first call the corresponding Handle method of the current aggregate to apply the changes of the current event,
        /// then append the current domain event into the current aggregate's internal queues.
        /// After the whole command handler is completed, ENode will pop all the events from the aggregate,
        /// and then persist the event to event store, and at last publish it to the query side.
        /// </remarks>
        /// </summary>
        /// <param name="domainEvent"></param>
        protected void ApplyEvent(IDomainEvent<TAggregateRootId> domainEvent)
        {
            if (domainEvent == null)
            {
                throw new ArgumentNullException("domainEvent");
            }
            if (Equals(this._id, default(TAggregateRootId)))
            {
                throw new Exception("Aggregate root id cannot be null.");
            }
            domainEvent.AggregateRootId = _id;
            domainEvent.Version = _version + 1;
            HandleEvent(domainEvent);
            AppendUncommittedEvent(domainEvent);
        }
        /// <summary>Apply multiple domain events to the current aggregate root.
        /// </summary>
        /// <param name="domainEvent"></param>
        protected void ApplyEvents(params IDomainEvent<TAggregateRootId>[] domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                ApplyEvent(domainEvent);
            }
        }

        private void HandleEvent(IDomainEvent domainEvent)
        {
            if (_eventHandlerProvider == null)
            {
                _eventHandlerProvider = ObjectContainer.Resolve<IAggregateRootInternalHandlerProvider>();
            }
            var handler = _eventHandlerProvider.GetInternalEventHandler(GetType(), domainEvent.GetType());
            if (handler == null)
            {
                throw new Exception(string.Format("Could not find event handler for [{0}] of [{1}]", domainEvent.GetType().FullName, GetType().FullName));
            }
            if (Equals(this._id, default(TAggregateRootId)) && domainEvent.Version == 1)
            {
                this._id = TypeUtils.ConvertType<TAggregateRootId>(domainEvent.AggregateRootStringId);
            }
            handler(this, domainEvent);
        }
        private void AppendUncommittedEvent(IDomainEvent<TAggregateRootId> domainEvent)
        {
            if (_uncommittedEvents == null)
            {
                _uncommittedEvents = new Queue<IDomainEvent>();
            }
            if (_uncommittedEvents.Any(x => x.GetType() == domainEvent.GetType()))
            {
                throw new InvalidOperationException(string.Format("Cannot apply duplicated domain event type: {0}, current aggregateRoot type: {1}, id: {2}", domainEvent.GetType().FullName, this.GetType().FullName, _id));
            }
            _uncommittedEvents.Enqueue(domainEvent);
        }
        private void VerifyEvent(DomainEventStream eventStream)
        {
            var current = this as IAggregateRoot;
            if (eventStream.Version > 1 && eventStream.AggregateRootId != current.UniqueId)
            {
                throw new InvalidOperationException(string.Format("Invalid domain event stream, aggregateRootId:{0}, expected aggregateRootId:{1}, type:{2}", eventStream.AggregateRootId, current.UniqueId, current.GetType().FullName));
            }
            if (eventStream.Version != current.Version + 1)
            {
                throw new InvalidOperationException(string.Format("Invalid domain event stream, version:{0}, expected version:{1}, current aggregateRoot type:{2}, id:{3}", eventStream.Version, current.Version, this.GetType().FullName, current.UniqueId));
            }
        }

        string IAggregateRoot.UniqueId
        {
            get
            {
                if (Id != null)
                {
                    return Id.ToString();
                }
                return null;
            }
        }
        int IAggregateRoot.Version
        {
            get { return _version; }
        }
        IEnumerable<IDomainEvent> IAggregateRoot.GetChanges()
        {
            if (_uncommittedEvents == null)
            {
                return _emptyEvents;
            }
            return _uncommittedEvents.ToArray();
        }
        void IAggregateRoot.AcceptChanges(int newVersion)
        {
            if (_version + 1 != newVersion)
            {
                throw new InvalidOperationException(string.Format("Cannot accept invalid version: {0}, expect version: {1}, current aggregateRoot type: {2}, id: {3}", newVersion, _version + 1, this.GetType().FullName, _id));
            }
            _version = newVersion;
            _uncommittedEvents.Clear();
        }
        void IAggregateRoot.ReplayEvents(IEnumerable<DomainEventStream> eventStreams)
        {
            if (eventStreams == null) return;

            foreach (var eventStream in eventStreams)
            {
                VerifyEvent(eventStream);
                foreach (var domainEvent in eventStream.Events)
                {
                    HandleEvent(domainEvent);
                }
                _version = eventStream.Version;
            }
        }
    }
}
