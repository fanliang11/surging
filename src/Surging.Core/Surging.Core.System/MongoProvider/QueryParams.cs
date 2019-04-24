using System;
using System.Collections.Generic;
using System.Text;

namespace Surging.Core.System.MongoProvider
{
    public class QueryParams
    {
        public QueryParams()
        {
            Index = 1;
            Size = 15;
        }

        public int Total { get; set; }

        public int Index { get; set; }

        public int Size { get; set; }

    }

    public class QueryParams<T>
    {
        public QueryParams()
        {
            Index = 1;
            Size = 15;
        }

        public int Total { get; set; }

        public T Params { get; set; }

        public int Index { get; set; }

        public int Size { get; set; }

    }
}
