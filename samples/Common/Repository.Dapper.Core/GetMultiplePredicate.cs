using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Repository.Dapper.Core
{
    public class GetMultiplePredicate
    {
        private readonly List<GetMultiplePredicateItem> _items;

        public GetMultiplePredicate()
        {
            _items = new List<GetMultiplePredicateItem>();
        }

        public IEnumerable<GetMultiplePredicateItem> Items
        {
            get { return _items.AsReadOnly(); }
        }

        public void Add<T>(IPredicate predicate, IList<ISort> sort = null) where T : class
        {
            _items.Add(new GetMultiplePredicateItem
                           {
                               Value = predicate,
                               Type = typeof(T),
                               Sort = sort
                           });
        }

        public void Add<T>(object id) where T : class
        {
            _items.Add(new GetMultiplePredicateItem
                           {
                               Value = id,
                               Type = typeof (T)
                           });
        }

        public class GetMultiplePredicateItem
        {
            public object Value { get; set; }
            public Type Type { get; set; }
            public IList<ISort> Sort { get; set; }
        }
    }
}