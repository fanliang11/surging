using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.CPlatform.Cache
{
    /// <summary>
    /// Defines the <see cref="CacheEndpoint" />
    /// </summary>
    public abstract class CacheEndpoint
    {
        #region 属性

        /// <summary>
        /// Gets or sets the Host
        /// 主机
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Gets or sets the Port
        /// 端口
        /// </summary>
        public int Port { get; set; }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The Equals
        /// </summary>
        /// <param name="obj">The obj<see cref="object"/></param>
        /// <returns>The <see cref="bool"/></returns>
        public override bool Equals(object obj)
        {
            var model = obj as CacheEndpoint;
            if (model == null)
                return false;

            if (obj.GetType() != GetType())
                return false;

            return model.ToString() == ToString();
        }

        /// <summary>
        /// The GetHashCode
        /// </summary>
        /// <returns>The <see cref="int"/></returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// The ToString
        /// </summary>
        /// <returns>The <see cref="string"/></returns>
        public abstract override string ToString();

        #endregion 方法

        public static bool operator ==(CacheEndpoint model1, CacheEndpoint model2)
        {
            return Equals(model1, model2);
        }

        public static bool operator !=(CacheEndpoint model1, CacheEndpoint model2)
        {
            return !Equals(model1, model2);
        }
    }
}