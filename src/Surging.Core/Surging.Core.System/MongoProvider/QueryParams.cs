using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.MongoProvider
{
    /// <summary>
    /// Defines the <see cref="QueryParams" />
    /// </summary>
    public class QueryParams
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParams"/> class.
        /// </summary>
        public QueryParams()
        {
            Index = 1;
            Size = 15;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the Size
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the Total
        /// </summary>
        public int Total { get; set; }

        #endregion 属性
    }

    /// <summary>
    /// Defines the <see cref="QueryParams{T}" />
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class QueryParams<T>
    {
        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryParams{T}"/> class.
        /// </summary>
        public QueryParams()
        {
            Index = 1;
            Size = 15;
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets or sets the Index
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the Params
        /// </summary>
        public T Params { get; set; }

        /// <summary>
        /// Gets or sets the Size
        /// </summary>
        public int Size { get; set; }

        /// <summary>
        /// Gets or sets the Total
        /// </summary>
        public int Total { get; set; }

        #endregion 属性
    }
}