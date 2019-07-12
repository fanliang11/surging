using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using Surging.Core.System.MongoProvider.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.MongoProvider
{
    /// <summary>
    /// Defines the <see cref="Util" />
    /// </summary>
    internal static class Util
    {
        #region 字段

        /// <summary>
        /// Defines the _config
        /// </summary>
        private static IConfigurationRoot _config;

        #endregion 字段

        #region 方法

        /// <summary>
        /// The GetCollectionFromConnectionString
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="connectionstring">The connectionstring<see cref="string"/></param>
        /// <returns>The <see cref="IMongoCollection{T}"/></returns>
        public static IMongoCollection<T> GetCollectionFromConnectionString<T>(string connectionstring)
            where T : IEntity
        {
            return GetDatabase(GetDefaultConnectionString()).GetCollection<T>(GetCollectionName<T>());
        }

        /// <summary>
        /// The GetDefaultConnectionString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public static string GetDefaultConnectionString()
        {
            return MongoConfig.DefaultInstance.MongConnectionString;
        }

        /// <summary>
        /// The GetCollectioNameFromInterface
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="string"/></returns>
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

        /// <summary>
        /// The GetCollectionName
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>The <see cref="string"/></returns>
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

        /// <summary>
        /// The GetCollectionNameFromType
        /// </summary>
        /// <param name="entitytype">The entitytype<see cref="Type"/></param>
        /// <returns>The <see cref="string"/></returns>
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

        /// <summary>
        /// The GetDatabase
        /// </summary>
        /// <param name="connectString">The connectString<see cref="string"/></param>
        /// <returns>The <see cref="IMongoDatabase"/></returns>
        private static IMongoDatabase GetDatabase(string connectString)
        {
            var mongoUrl = new MongoUrl(connectString);
            var client = new MongoClient(connectString);
            return client.GetDatabase(mongoUrl.DatabaseName);
        }

        #endregion 方法
    }
}