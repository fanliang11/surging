using StackExchange.Redis;
using Surging.Core.CPlatform.Cache;
using Surging.Core.Redis.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Surging.Core.Redis.Provider.Implementation
{
    public class RedisProvider : ICacheProvider
    {
        public long DefaultExpireTime { get; set; } = long.MaxValue;
        public string KeySuffix { get; set; }

        private readonly RedisEndpoint _endpoint;

        private readonly int _connectTimeout=0;

        public ICacheClient _cacheClient { get; set; }

        public IDatabase ICacheClient { get; set; }

        public RedisProvider(RedisEndpoint endpoint, ICacheClient client)
        {
             KeySuffix = endpoint.KeySuffix;
            _endpoint = endpoint;
            _cacheClient = client;
        }

        #region 公共方法
        /// <summary>
        /// 添加K/V值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void Add(string key, object value)
        {
            var redis = _cacheClient.GetClient(_endpoint, _connectTimeout);
            redis.Set(GetKeySuffix(key), value);
        }

        /// <summary>
        /// 异步添加K/V值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public async Task AddAsync(string key, object value)
        {
            var redis = _cacheClient.GetClient(_endpoint, _connectTimeout);
            await redis.SetAsync(GetKeySuffix(key), value);
        }

        /// <summary>
        /// 添加k/v值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="defaultExpire">默认配置失效时间</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void Add(string key, object value, bool defaultExpire)
        {
            this.Add(key, value, TimeSpan.FromMinutes(defaultExpire ? DefaultExpireTime : ExpireTime));
        }

        /// <summary>
        /// 异步添加K/V值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="defaultExpire">默认配置失效时间</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public async Task AddAsync(string key, object value, bool defaultExpire)
        {
            var redis = _cacheClient.GetClient(_endpoint, _connectTimeout);
            await redis.SetAsync(GetKeySuffix(key), value);
        }

        /// <summary>
        /// 添加k/v值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="numOfMinutes">默认配置失效时间</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void Add(string key, object value, long numOfMinutes)
        {

            this.Add(key, value, TimeSpan.FromMinutes(numOfMinutes));
        }


        /// <summary>
        /// 异步添加K/V值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="numOfMinutes">默认配置失效时间</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public async Task AddAsync(string key, object value, long numOfMinutes)
        {
            var redis = _cacheClient.GetClient(_endpoint, _connectTimeout);
            await redis.SetAsync(GetKeySuffix(key), value, TimeSpan.FromMinutes(numOfMinutes));
        }
        
        /// <summary>
        /// 添加k/v值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="timeSpan">配置时间间隔</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void Add(string key, object value, TimeSpan timeSpan)
        {
            var redis = _cacheClient.GetClient(_endpoint, _connectTimeout);
            redis.Set(GetKeySuffix(key), value, timeSpan);
        }

        /// <summary>
        /// 异步添加K/V值
        /// </summary>
        /// <param name="key">键</param>
        /// <param name="value">值</param>
        /// <param name="timeSpan">配置时间间隔</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void AddAsync(string key, object value, TimeSpan timeSpan)
        {
            var redis = _cacheClient.GetClient(_endpoint, _connectTimeout);
            redis.Set(GetKeySuffix(key), value, timeSpan);
        }

        /// <summary>
        /// 根据KEY键集合获取返回对象集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="keys">KEY值集合</param>
        /// <returns>需要返回的对象集合</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public IDictionary<string, T> Get<T>(IEnumerable<string> keys)
        {
            IDictionary<string, T> result = null;
            foreach (var key in keys)
            {

                var node = GetRedisNode(key);
                var redis = GetRedisClient(new RedisEndpoint()
                {
                    DbIndex = int.Parse(node.Db),
                    Host = node.Host,
                    Password = node.Password,
                    Port = int.Parse(node.Port),
                    MinSize = int.Parse(node.MinSize),
                    MaxSize = int.Parse(node.MaxSize),
                });
                result.Add(key, redis.Get<T>(key));
            }
            return result;
        }

        /// <summary>
        /// 根据KEY键集合异步获取返回对象集合
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="keys">KEY值集合</param>
        /// <returns>需要返回的对象集合</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public async Task<IDictionary<string, T>> GetAsync<T>(IEnumerable<string> keys)
        {
            IDictionary<string, T> result = null;
            foreach (var key in keys)
            {

                var node = GetRedisNode(key);
                var redis = GetRedisClient(new RedisEndpoint()
                {
                    DbIndex = int.Parse(node.Db),
                    Host = node.Host,
                    Password = node.Password,
                    Port = int.Parse(node.Port),
                    MinSize = int.Parse(node.MinSize),
                    MaxSize = int.Parse(node.MaxSize),
                });
                result.Add(key, await redis.GetAsync<T>(key));
            }
            return result;
        }

        /// <summary>
        /// 根据KEY键获取返回对象
        /// </summary>
        /// <param name="key">KEY值</param>
        /// <returns>需要返回的对象</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public object Get(string key)
        {
            var o = this.Get<object>(key);
            return o;
        }

        /// <summary>
        /// 根据KEY异步获取返回对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<object> GetAsync(string key)
        {
            var result = await this.GetTaskAsync<object>(key);
            return result;
        }

        /// <summary>
        /// 根据KEY键获取返回指定的类型对象
        /// </summary>
        /// <typeparam name="T">返回类型</typeparam>
        /// <param name="key">KEY值</param>
        /// <returns>需要返回的对象</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public T Get<T>(string key)
        {
            var node = GetRedisNode(key);
            var result = default(T);
            var redis = GetRedisClient(new RedisEndpoint()
            {
                DbIndex = int.Parse(node.Db),
                Host = node.Host,
                Password = node.Password,
                Port = int.Parse(node.Port), 
            });
            result = redis.Get<T>(GetKeySuffix(key));
            return result;
        }



        /// <summary>
        /// 根据KEY异步获取指定的类型对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<T> GetAsync<T>(string key)
        {
            var node = GetRedisNode(key);
            var redis = GetRedisClient(new RedisEndpoint()
            {
                DbIndex = int.Parse(node.Db),
                Host = node.Host,
                Password = node.Password,
                Port = int.Parse(node.Port),
                MinSize = int.Parse(node.MinSize),
                MaxSize = int.Parse(node.MaxSize),
            });

            var result = await Task.Run(() => redis.Get<T>(GetKeySuffix(key)));
            return result;

        }

        /// <summary>
        /// 根据KEY键获取转化成指定的对象，指示获取转化是否成功的返回值
        /// </summary>
        /// <param name="key">KEY键</param>
        /// <param name="obj">需要转化返回的对象</param>
        /// <returns>是否成功</returns>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public bool GetCacheTryParse(string key, out object obj)
        {
            obj = null;
            var o = this.Get<object>(key);
            return o != null;
        }

        /// <summary>
        /// 根据KEY键删除缓存
        /// </summary>
        /// <param name="key">KEY键</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void Remove(string key)
        {
            var node = GetRedisNode(key);
            var redis = GetRedisClient(new RedisEndpoint()
            {
                DbIndex = int.Parse(node.Db),
                Host = node.Host,
                Password = node.Password,
                Port = int.Parse(node.Port),
                MinSize = int.Parse(node.MinSize),
                MaxSize = int.Parse(node.MaxSize),
            });
            redis.Remove(GetKeySuffix(key));

        }

        /// <summary>
        /// 根据KEY键异步删除缓存
        /// </summary>
        /// <param name="key">KEY键</param>
        /// <remarks>
        /// 	<para>创建：范亮</para>
        /// 	<para>日期：2016/4/2</para>
        /// </remarks>
        public void RemoveAsync(string key)
        {
            this.RemoveTaskAsync(key);
        }




        #endregion



        private string GetKeySuffix(string key)
        {
            return string.IsNullOrEmpty(KeySuffix) ? key : string.Format("_{0}_{1}", KeySuffix, key);
        }
    }
}
