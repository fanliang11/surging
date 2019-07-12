using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Surging.Core.Caching.HashAlgorithms
{
    /// <summary>
    /// 针对<see cref="T"/>哈希算法实现
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public class ConsistentHash<T>
    {
        #region 字段

        /// <summary>
        /// Defines the _hashAlgorithm
        /// </summary>
        private readonly IHashAlgorithm _hashAlgorithm;

        /// <summary>
        /// Defines the _ring
        /// </summary>
        private readonly SortedDictionary<int, T> _ring = new SortedDictionary<int, T>();

        /// <summary>
        /// Defines the _virtualNodeReplicationFactor
        /// </summary>
        private readonly int _virtualNodeReplicationFactor = 1000;

        /// <summary>
        /// Defines the _nodeKeysInRing
        /// </summary>
        private int[] _nodeKeysInRing = null;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsistentHash{T}"/> class.
        /// </summary>
        /// <param name="hashAlgorithm">The hashAlgorithm<see cref="IHashAlgorithm"/></param>
        public ConsistentHash(IHashAlgorithm hashAlgorithm)
        {
            _hashAlgorithm = hashAlgorithm;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConsistentHash{T}"/> class.
        /// </summary>
        /// <param name="hashAlgorithm">The hashAlgorithm<see cref="IHashAlgorithm"/></param>
        /// <param name="virtualNodeReplicationFactor">The virtualNodeReplicationFactor<see cref="int"/></param>
        public ConsistentHash(IHashAlgorithm hashAlgorithm, int virtualNodeReplicationFactor)
            : this(hashAlgorithm)
        {
            _virtualNodeReplicationFactor = virtualNodeReplicationFactor;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the VirtualNodeReplicationFactor
        /// 复制哈希节点数
        /// </summary>
        public int VirtualNodeReplicationFactor
        {
            get { return _virtualNodeReplicationFactor; }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="node">节点</param>
        /// <param name="value">The value<see cref="string"/></param>
        public void Add(T node, string value)
        {
            AddNode(node, value);
            _nodeKeysInRing = _ring.Keys.ToArray();
        }

        /// <summary>
        /// 通过哈希算法计算出对应的节点
        /// </summary>
        /// <param name="item">值</param>
        /// <returns>返回节点</returns>
        public T GetItemNode(string item)
        {
            var hashOfItem = _hashAlgorithm.Hash(item);
            var nearestNodePosition = GetClockwiseNearestNode(_nodeKeysInRing, hashOfItem);
            return _ring[_nodeKeysInRing[nearestNodePosition]];
        }

        /// <summary>
        /// The GetNodes
        /// </summary>
        /// <returns>The <see cref="IEnumerable{T}"/></returns>
        public IEnumerable<T> GetNodes()
        {
            return _ring.Values.Distinct().ToList();
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="node">节点</param>
        public void Remove(string node)
        {
            RemoveNode(node);
            _nodeKeysInRing = _ring.Keys.ToArray();
        }

        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="node">节点</param>
        /// <param name="value">The value<see cref="string"/></param>
        private void AddNode(T node, string value)
        {
            for (var i = 0; i < _virtualNodeReplicationFactor; i++)
            {
                var hashOfVirtualNode = _hashAlgorithm.Hash(value.ToString(CultureInfo.InvariantCulture) + i);
                _ring[hashOfVirtualNode] = node;
            }
        }

        /// <summary>
        /// 顺时针查找对应哈希的位置
        /// </summary>
        /// <param name="keys">键集合数</param>
        /// <param name="hashOfItem">哈希值</param>
        /// <returns>返回哈希的位置</returns>
        private int GetClockwiseNearestNode(int[] keys, int hashOfItem)
        {
            var begin = 0;
            var end = keys.Length - 1;
            if (keys[end] < hashOfItem || keys[0] > hashOfItem)
            {
                return 0;
            }
            var mid = begin;
            while ((end - begin) > 1)
            {
                mid = (end + begin) / 2;
                if (keys[mid] >= hashOfItem) end = mid;
                else begin = mid;
            }
            return end;
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        /// <param name="value">The value<see cref="string"/></param>
        private void RemoveNode(string value)
        {
            for (var i = 0; i < _virtualNodeReplicationFactor; i++)
            {
                var hashOfVirtualNode = _hashAlgorithm.Hash(value.ToString() + i);
                _ring.Remove(hashOfVirtualNode);
            }
        }

        #endregion 方法
    }
}