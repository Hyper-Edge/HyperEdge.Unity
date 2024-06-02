using System;
using System.Collections.Generic;
using System.Linq;


namespace HyperEdge.Sdk.Server
{
    public class ChangeSet
    {
        private Dictionary<Ulid, IModel<Ulid>> _added;
        private Dictionary<Ulid, IModel<Ulid>> _updated;
        private Dictionary<Ulid, IModel<Ulid>> _removed;

        private Dictionary<Tuple<Ulid, Ulid>, ulong> _addedItems;
        private Dictionary<Tuple<Ulid, Ulid>, ulong> _updatedItems;

        public IEnumerable<Tuple<Ulid, Ulid, ulong>> AddedItems
        {
            get => _addedItems?.Select(el => Tuple.Create(el.Key.Item1, el.Key.Item2, el.Value)) ?? Enumerable.Empty<Tuple<Ulid, Ulid, ulong>>();
        }

        public IEnumerable<Tuple<Ulid, Ulid, ulong>> UpdatedItems
        {
            get => _updatedItems?.Select(el => Tuple.Create(el.Key.Item1, el.Key.Item2, el.Value)) ?? Enumerable.Empty<Tuple<Ulid, Ulid, ulong>>();
        }

        public IEnumerable<IModel<Ulid>> Added
        {
            get => _added?.Values ?? Enumerable.Empty<IModel<Ulid>>();
        }

        public IEnumerable<IModel<Ulid>> Updated
        {
            get => _updated?.Values ?? Enumerable.Empty<IModel<Ulid>>();
        }

        public IEnumerable<IModel<Ulid>> Removed
        {
            get => _removed?.Values ?? Enumerable.Empty<IModel<Ulid>>();
        }

        public void Add(IModel<Ulid> m)
        {
            var added = _added ?? (_added = new Dictionary<Ulid, IModel<Ulid>>());
            if (!added.ContainsKey(m.Id))
            {
                added.Add(m.Id, m);
            }
        }

        public void Update(IModel<Ulid> m)
        {
            var updated = _updated ?? (_updated = new Dictionary<Ulid, IModel<Ulid>>());
            if (!updated.ContainsKey(m.Id))
            {
                updated.Add(m.Id, m);
            }
        }

        public void Remove(IModel<Ulid> m)
        { 
            var removed = _removed ?? (_removed = new Dictionary<Ulid, IModel<Ulid>>());
            if (!removed.ContainsKey(m.Id))
            {
                removed.Add(m.Id, m);
            }
        }

        public void AddItems(Ulid userId, Ulid itemId, ulong amount)
        {
            var key = Tuple.Create(userId, itemId);
            var addedItems = _addedItems ?? (_addedItems = new Dictionary<Tuple<Ulid, Ulid>, ulong>());
            if (_addedItems.ContainsKey(key))
            {
                _addedItems[key] = amount;
            }
            else
            {
                _addedItems.Add(key, amount);
            }
        }

        public void UpdateItems(Ulid userId, Ulid itemId, ulong amount)
        {
            var key = Tuple.Create(userId, itemId);
            if (_addedItems is not null && _addedItems.ContainsKey(key))
            {
                _addedItems[key] = amount;
                return;
            }
            var updatedItems = _updatedItems ?? (_updatedItems = new Dictionary<Tuple<Ulid, Ulid>, ulong>());
            if (_updatedItems.ContainsKey(key))
            {
                _updatedItems[key] = amount;
            }
            else
            {
                _updatedItems.Add(key, amount);
            }
        }
    }
}
