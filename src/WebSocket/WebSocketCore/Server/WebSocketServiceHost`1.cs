/*
 * WebSocketServiceHost`1.cs
 *
 * The MIT License
 *
 * Copyright (c) 2015-2017 sta.blockhead
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */

using System;

namespace WebSocketCore.Server
{
    /// <summary>
    /// Defines the <see cref="WebSocketServiceHost{TBehavior}" />
    /// </summary>
    /// <typeparam name="TBehavior"></typeparam>
    internal class WebSocketServiceHost<TBehavior> : WebSocketServiceHostBase
    where TBehavior : WebSocketBehavior
    {
        #region 字段

        /// <summary>
        /// Defines the _creator
        /// </summary>
        private Func<TBehavior> _creator;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServiceHost{TBehavior}"/> class.
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="creator">The creator<see cref="Func{TBehavior}"/></param>
        /// <param name="log">The log<see cref="Logger"/></param>
        internal WebSocketServiceHost(
      string path, Func<TBehavior> creator, Logger log
    )
      : this(path, creator, null, log)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServiceHost{TBehavior}"/> class.
        /// </summary>
        /// <param name="path">The path<see cref="string"/></param>
        /// <param name="creator">The creator<see cref="Func{TBehavior}"/></param>
        /// <param name="initializer">The initializer<see cref="Action{TBehavior}"/></param>
        /// <param name="log">The log<see cref="Logger"/></param>
        internal WebSocketServiceHost(
      string path,
      Func<TBehavior> creator,
      Action<TBehavior> initializer,
      Logger log
    )
      : base(path, log)
        {
            _creator = createCreator(creator, initializer);
        }

        #endregion 构造函数

        #region 属性

        /// <summary>
        /// Gets the BehaviorType
        /// </summary>
        public override Type BehaviorType
        {
            get
            {
                return typeof(TBehavior);
            }
        }

        #endregion 属性

        #region 方法

        /// <summary>
        /// The CreateSession
        /// </summary>
        /// <returns>The <see cref="WebSocketBehavior"/></returns>
        protected override WebSocketBehavior CreateSession()
        {
            return _creator();
        }

        /// <summary>
        /// The createCreator
        /// </summary>
        /// <param name="creator">The creator<see cref="Func{TBehavior}"/></param>
        /// <param name="initializer">The initializer<see cref="Action{TBehavior}"/></param>
        /// <returns>The <see cref="Func{TBehavior}"/></returns>
        private Func<TBehavior> createCreator(
      Func<TBehavior> creator, Action<TBehavior> initializer
    )
        {
            if (initializer == null)
                return creator;

            return () =>
            {
                var ret = creator();
                initializer(ret);

                return ret;
            };
        }

        #endregion 方法
    }
}