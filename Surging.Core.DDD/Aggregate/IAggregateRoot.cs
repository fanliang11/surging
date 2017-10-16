using System.Collections.Generic;

namespace Surging.Core.DDD
{
    /// <summary>Represents an aggregate root.
    /// </summary>
    public  interface IAggregateRoot
    {
        /// <summary>Represents the unique id of the aggregate root.
        /// </summary>
        string UniqueId { get; }
        /// <summary>Represents the current version of the aggregate root.
        /// </summary>
        int Version { get; }
        /// <summary>Get all the changes of the aggregate root.
        /// </summary>
        /// <returns></returns>
        IEnumerable<IDomainEvent> GetChanges();
        /// <summary>Accept changes with new version.
        /// </summary>
        /// <param name="newVersion"></param>
        void AcceptChanges(int newVersion);
        /// <summary>Replay the given event streams.
        /// </summary>
        /// <param name="eventStreams"></param>
        void ReplayEvents(IEnumerable<DomainEventStream> eventStreams);
    }
}
