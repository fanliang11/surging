using Surging.Core.CPlatform.Engines;
using Surging.Core.CPlatform.Runtime.Server;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Surging.Core.Swagger.Internal
{
    /// <summary>
    /// Defines the <see cref="DefaultServiceSchemaProvider" />
    /// </summary>
    public class DefaultServiceSchemaProvider : IServiceSchemaProvider
    {
        #region 字段

        /// <summary>
        /// Defines the _serviceEntryProvider
        /// </summary>
        private readonly IServiceEntryProvider _serviceEntryProvider;

        #endregion 字段

        #region 构造函数

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultServiceSchemaProvider"/> class.
        /// </summary>
        /// <param name="serviceEntryProvider">The serviceEntryProvider<see cref="IServiceEntryProvider"/></param>
        public DefaultServiceSchemaProvider(IServiceEntryProvider serviceEntryProvider)
        {
            _serviceEntryProvider = serviceEntryProvider;
        }

        #endregion 构造函数

        #region 方法

        /// <summary>
        /// The GetSchemaFilesPath
        /// </summary>
        /// <returns>The <see cref="IEnumerable{string}"/></returns>
        public IEnumerable<string> GetSchemaFilesPath()
        {
            var result = new List<string>();
            var assemblieFiles = _serviceEntryProvider.GetALLEntries()
                        .Select(p => p.Type.Assembly.Location).Distinct();

            foreach (var assemblieFile in assemblieFiles)
            {
                var fileSpan = assemblieFile.AsSpan();
                var path = $"{fileSpan.Slice(0, fileSpan.LastIndexOf(".")).ToString()}.xml";
                if (File.Exists(path))
                    result.Add(path);
            }
            return result;
        }

        #endregion 方法
    }
}