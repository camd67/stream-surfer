using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreamSurfer.Services
{
    public class RotatingCache<T>
    {
        private Dictionary<string, T> cache;
        private Queue<string> keyQueue;
        private int maxSize;

        public RotatingCache(int maxSize = 50)
        {
            this.maxSize = maxSize;
            cache = new Dictionary<string, T>(maxSize + 1);
            keyQueue = new Queue<string>(maxSize + 1);
        }

        public void Add(string key, T toAdd)
        {
            key = key.ToLower();
            if (cache.ContainsKey(key)) { return; }
            if(keyQueue.Count >= maxSize)
            {
                string toRem = keyQueue.Dequeue();
                cache.Remove(toRem);
            }
            keyQueue.Enqueue(key);
            cache.Add(key, toAdd);
        }

        public T Get(string key)
        {
            if (cache.ContainsKey(key))
            {
                return cache[key];
            }
            else
            {
                return default(T);
            }
        }

        public void Clear()
        {
            keyQueue.Clear();
            cache.Clear();
        }
    }
}
