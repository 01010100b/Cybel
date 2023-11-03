using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cybel.Core
{
    public static class ObjectPool
    {
        private const int MAX_POOL_SIZE = 1000000;

        private static ConcurrentDictionary<Type, ConcurrentQueue<object>> Pools { get; } = new();

        public static T Get<T>(Func<T> create, Action<T>? reset = null)
        {
            var pool = GetPool(typeof(T));

            if (pool.TryDequeue(out var obj))
            {
                var o = (T)obj;

                if (reset is not null)
                {
                    reset(o);
                }

                return o;
            }
            else
            {
                return create();
            }
        }

        public static void Add(object obj)
        {
            var pool = GetPool(obj.GetType());

            if (pool.Count < MAX_POOL_SIZE)
            {
                pool.Enqueue(obj);
            }
        }

        private static ConcurrentQueue<object> GetPool(Type type) => Pools.GetOrAdd(type, x => new());
    }
}
