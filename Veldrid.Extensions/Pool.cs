using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Veldrid.Extensions
{
    public class Pool<T> : IEnumerable<T>, IDisposable where T: IDisposable
    {
        private ConcurrentBag<T> _available;
        private Func<T> _generator;
        private ulong _capacity;

        public Pool(Func<T> generator)
        {
            if (generator == null) throw new ArgumentNullException(nameof(generator));
            _available = new ConcurrentBag<T>();
            _generator = generator;
        }

        public IDisposable<T> Take()
        {
            T item;
            if (_available.TryTake(out item))
                return new DisposableWrapper<T>(item, val => Put(val));

            _capacity++;
            return new DisposableWrapper<T>(_generator(), val => Put(val));
        }

        public IDisposable<T> Take(out T value)
        {
            var result = Take();
            value = result.Value;

            return result;
        }

        public void Put(T item)
        {
            _available.Add(item);
        }

        public Pool<T> Reserve(ulong count)
        {
            for (var i = 0u; i < count; i++)
                _available.Add(_generator());

            _capacity = count;
            return this;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _available.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _available.GetEnumerator();
        }

        public void Dispose()
        {
            foreach (var item in _available)
                item.Dispose();
        }
    }
}