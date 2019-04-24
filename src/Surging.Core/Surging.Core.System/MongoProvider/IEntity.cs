using System;
using MongoDB.Bson.Serialization.Attributes;

namespace Surging.Core.System.MongoProvider
{
    public interface IEntity
    {
        [BsonId]
        string Id { get; set; }
    }
}
