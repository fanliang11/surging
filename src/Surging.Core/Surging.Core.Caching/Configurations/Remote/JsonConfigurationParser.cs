using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Surging.Core.Caching.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Surging.Core.Caching.Configurations.Remote
{
    /// <summary>
    /// Defines the <see cref="JsonConfigurationParser" />
    /// </summary>
    public class JsonConfigurationParser : IConfigurationParser
    {
        #region 字段

        /// <summary>
        /// Defines the _context
        /// </summary>
        private readonly Stack<string> _context = new Stack<string>();

        /// <summary>
        /// Defines the _data
        /// </summary>
        private readonly IDictionary<string, string> _data = new SortedDictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Defines the _currentPath
        /// </summary>
        private string _currentPath;

        /// <summary>
        /// Defines the _reader
        /// </summary>
        private JsonTextReader _reader;

        #endregion 字段

        #region 方法

        /// <summary>
        /// The Parse
        /// </summary>
        /// <param name="input">The input<see cref="Stream"/></param>
        /// <param name="initialContext">The initialContext<see cref="string"/></param>
        /// <returns>The <see cref="IDictionary{string, string}"/></returns>
        public IDictionary<string, string> Parse(Stream input, string initialContext)
        {
            try
            {
                _data.Clear();
                _reader = new JsonTextReader(new StreamReader(input));
                _reader.DateParseHandling = DateParseHandling.None;
                var jsonConfig = JObject.Load(_reader);
                if (!string.IsNullOrEmpty(initialContext)) { EnterContext(initialContext); }
                VisitJObject(jsonConfig);
                if (!string.IsNullOrEmpty(initialContext)) { ExitContext(); }

                return _data;
            }
            catch (JsonReaderException e)
            {
                string errorLine = string.Empty;
                if (input.CanSeek)
                {
                    input.Seek(0, SeekOrigin.Begin);
                    IEnumerable<string> fileContent;
                    using (var streamReader = new StreamReader(input))
                    {
                        fileContent = ReadLines(streamReader);
                        errorLine = RetrieveErrorContext(e, fileContent);
                    }
                }
                throw new FormatException(string.Format(
                        CachingResources.JSONParseException,
                        e.LineNumber,
                        errorLine),
                    e);
            }
        }

        /// <summary>
        /// The ReadLines
        /// </summary>
        /// <param name="streamReader">The streamReader<see cref="StreamReader"/></param>
        /// <returns>The <see cref="IEnumerable{string}"/></returns>
        private static IEnumerable<string> ReadLines(StreamReader streamReader)
        {
            string line;
            do
            {
                line = streamReader.ReadLine();
                yield return line;
            } while (line != null);
        }

        /// <summary>
        /// The RetrieveErrorContext
        /// </summary>
        /// <param name="e">The e<see cref="JsonReaderException"/></param>
        /// <param name="fileContent">The fileContent<see cref="IEnumerable{string}"/></param>
        /// <returns>The <see cref="string"/></returns>
        private static string RetrieveErrorContext(JsonReaderException e, IEnumerable<string> fileContent)
        {
            string errorLine;
            if (e.LineNumber >= 2)
            {
                var errorContext = fileContent.Skip(e.LineNumber - 2).Take(2).ToList();
                errorLine = errorContext[0].Trim() + Environment.NewLine + errorContext[1].Trim();
            }
            else
            {
                var possibleLineContent = fileContent.Skip(e.LineNumber - 1).FirstOrDefault();
                errorLine = possibleLineContent ?? string.Empty;
            }
            return errorLine;
        }

        /// <summary>
        /// The EnterContext
        /// </summary>
        /// <param name="context">The context<see cref="string"/></param>
        private void EnterContext(string context)
        {
            _context.Push(context);
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        /// <summary>
        /// The ExitContext
        /// </summary>
        private void ExitContext()
        {
            _context.Pop();
            _currentPath = ConfigurationPath.Combine(_context.Reverse());
        }

        /// <summary>
        /// The VisitArray
        /// </summary>
        /// <param name="array">The array<see cref="JArray"/></param>
        private void VisitArray(JArray array)
        {
            for (int index = 0; index < array.Count; index++)
            {
                EnterContext(index.ToString());
                VisitToken(array[index]);
                ExitContext();
            }
        }

        /// <summary>
        /// The VisitJObject
        /// </summary>
        /// <param name="jObject">The jObject<see cref="JObject"/></param>
        private void VisitJObject(JObject jObject)
        {
            foreach (var property in jObject.Properties())
            {
                EnterContext(property.Name);
                VisitProperty(property);
                ExitContext();
            }
        }

        /// <summary>
        /// The VisitPrimitive
        /// </summary>
        /// <param name="data">The data<see cref="JToken"/></param>
        private void VisitPrimitive(JToken data)
        {
            var key = _currentPath;
            Check.CheckCondition(() => _data.ContainsKey(key), "key");
            _data[key] = data.ToString();
        }

        /// <summary>
        /// The VisitProperty
        /// </summary>
        /// <param name="property">The property<see cref="JProperty"/></param>
        private void VisitProperty(JProperty property)
        {
            VisitToken(property.Value);
        }

        /// <summary>
        /// The VisitToken
        /// </summary>
        /// <param name="token">The token<see cref="JToken"/></param>
        private void VisitToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    VisitJObject(token.Value<JObject>());
                    break;

                case JTokenType.Array:
                    VisitArray(token.Value<JArray>());
                    break;

                case JTokenType.Integer:
                case JTokenType.Float:
                case JTokenType.String:
                case JTokenType.Boolean:
                case JTokenType.Bytes:
                case JTokenType.Raw:
                case JTokenType.Null:
                    VisitPrimitive(token);
                    break;

                default:
                    throw new FormatException(string.Format(
                       CachingResources.UnsupportedJSONToken,
                        _reader.TokenType,
                        _reader.Path,
                        _reader.LineNumber,
                        _reader.LinePosition));
            }
        }

        #endregion 方法
    }
}