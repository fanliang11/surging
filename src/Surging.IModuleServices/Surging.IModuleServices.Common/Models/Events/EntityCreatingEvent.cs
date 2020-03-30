using Surging.Core.CPlatform.EventBus.Events;

namespace Surging.IModuleServices.Common.Models.Events
{
    /// <summary>
    /// Entity Creating Event
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    public class EntityCreatingEvent<TEntity> : IntegrationEvent
    {
        /// <summary>
        /// 实体
        /// </summary>
        public TEntity Entity { get; set; }
    }
}
