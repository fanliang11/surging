using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Surging.Core.System.MongoProvider.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.MongoProvider
{
    internal static class Util
    {
        private static IConfigurationRoot _config;
        private static IMongoDatabase GetDatabase(string connectString)
        {
            var mongoUrl = new MongoUrl(connectString);
            var client = new MongoClient(connectString);
            return client.GetDatabase(mongoUrl.DatabaseName);

        }

        public static string GetDefaultConnectionString()
        {
            return MongoConfig.DefaultInstance.MongConnectionString;
        }

        public static IMongoCollection<T> GetCollectionFromConnectionString<T>(string connectionstring)
            where T : IEntity
        {
            return GetDatabase(GetDefaultConnectionString()).GetCollection<T>(GetCollectionName<T>());
        }

        private static string GetCollectionName<T>() where T : IEntity
        {
            string collectionName;
            if (typeof(T).BaseType.Equals(typeof(object)))
            {
                collectionName = GetCollectioNameFromInterface<T>();
            }
            else
            {
                collectionName = GetCollectionNameFromType(typeof(T));
            }

            if (string.IsNullOrEmpty(collectionName))
            {
                throw new ArgumentException("这个实体的集合名称不能为空");
            }
            return collectionName;
        }

        private static string GetCollectioNameFromInterface<T>()
        {
            string collectionname;
            Attribute att = Attribute.GetCustomAttribute(typeof(T), typeof(CollectionNameAttribute));
            if (att != null)
            {
                collectionname = ((CollectionNameAttribute)att).Name;
            }
            else
            {
                collectionname = typeof(T).Name;
            }

            return collectionname;
        }

        private static string GetCollectionNameFromType(Type entitytype)
        {
            string collectionname;
            Attribute att = Attribute.GetCustomAttribute(entitytype, typeof(CollectionNameAttribute));
            if (att != null)
            {
                collectionname = ((CollectionNameAttribute)att).Name;
            }
            else
            {
                while (!entitytype.BaseType.Equals(typeof(Entity)))
                {
                    entitytype = entitytype.BaseType;
                }
                collectionname = entitytype.Name;
            }
            return collectionname;
        }
    }
}

