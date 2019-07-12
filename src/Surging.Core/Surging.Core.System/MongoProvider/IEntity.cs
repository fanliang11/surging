using MongoDB.Bson.Serialization.Attributes;
using System;

namespace Surging.Core.System.MongoProvider
{
    #region 接口

    /// <summary>
    /// Defines the <see cref="IEntity" />
    /// </summary>
    public interface IEntity
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Id
        /// </summary>
        [BsonId]
        string Id { get; set; }

        #endregion 属性
    }

    #endregion 接口
}