// Copyright 2007-2010 The Apache Software Foundation.
// 
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use 
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed 
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.
namespace Magnum.Tail
{
    using System;
    using System.IO;
    using System.Threading;

    public class Tail :
        IDisposable
    {
        bool _run;
        readonly Action<string> _action;
        readonly string _filePath;

        public static Tail New(string filePath, Action<TailConfiguration> cfgAction)
        {
            var cfg = new TailConfigurationX(filePath);
            cfgAction(cfg);
            return cfg.Build();
        }

        /// <summary>
        ///   Use Tail.New(path, cfg);
        /// </summary>
        internal Tail(string filePath, Action<string> action)
        {
            _filePath = filePath;
            _action = action;
        }

        public void Start()
        {
            _run = true;
            using (var reader = new StreamReader(new FileStream(_filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
            {
                //start at the beginning
                long lastOffSet = 0;

                //feels like actors would be better here
                while (_run)
                {
                    Thread.Sleep(100);

                    if (reader.BaseStream.Length == lastOffSet)
                        continue;

                    reader.BaseStream.Seek(lastOffSet, SeekOrigin.Begin);

                    string line = "";

                    while ((line = reader.ReadLine()) != null)
                    {
                        _action(line);
                    }

                    lastOffSet = reader.BaseStream.Position;
                }
            }
        }

        public void Stop()
        {
            _run = false;
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public interface TailConfiguration
    {
        void AddAction(Action<string> action);
    }

    public class TailConfigurationX :
        TailConfiguration
    {
        static string _file;

        Action<string> _actions = s =>
        {
        };

        public TailConfigurationX(string filePath)
        {
            _file = filePath;
        }

        public void AddAction(Action<string> action)
        {
            _actions += action;
        }

        public Tail Build()
        {
            return new Tail(_file, _actions);
        }
    }
}