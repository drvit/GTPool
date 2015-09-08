using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using GTPool;
using GTP = GTPool.GenericThreadPool;

namespace NewsSearch.Infrastructure.Utils
{
    public sealed class GlobalCache : IDisposable
    {
        private static readonly GlobalCache _current = new GlobalCache();
        private readonly object _locker = new object();
        private static readonly ConcurrentBag<GlobalRepository> _repository = new ConcurrentBag<GlobalRepository>();
        private readonly ManagedJob _job;
        private bool _stopGarbageCollection;

        static GlobalCache() { }

        private GlobalCache()
        {
            if (_job == null)
            {
                _job = new ManagedJob((Action)GarbageCollector);
                GTP.AddJob(_job);
            }
        }

        public void GarbageCollector()
        {
            while (!_stopGarbageCollection)
            {
                var tokens = _repository.Where(x => x.LastUpdate.AddMinutes(15) < DateTime.UtcNow)
                    .Select(x => x.Token).ToList();

                foreach (var token in tokens)
                {
                    Remove(token);
                }

                lock (_locker)
                {
                    Monitor.Wait(_locker, 60000);
                }
            }
        }

        public static void StopGarbageCollector()
        {
            _current._stopGarbageCollection = true;
        }

        public static GlobalCache Current { get { return _current; } }

        public static void Add(string token, string key, object value)
        {
            var rep = _repository.FirstOrDefault(x => x.Token == token);
            if (rep == null)
            {
                rep = new GlobalRepository(token);
                rep.SetFieldValue(key, value);

                _repository.Add(rep);
            }

            rep.SetFieldValue(key, value);
        }

        public static object Get(string token, string key)
        {
            var rep = _repository.FirstOrDefault(x => Equals(x.Token, token));
            
            return rep != null ? rep.GetFieldValue(key) : null;
        }

        public static object GetOnce(string token, string key)
        {
            var rep = _repository.FirstOrDefault(x => Equals(x.Token, token));
            
            if (rep == null) 
                return null;

            var ret = rep.GetFieldValue(key);
            Remove(token, key);

            return ret;
        }

        public static void Remove(string token, string key)
        {
            var rep = _repository.FirstOrDefault(x => Equals(x.Token, token));
            if (rep != null)
            {
                rep.RemoveField(key);

                if (_repository.Count(x => Equals(x.Token, token)) == 0)
                    Remove(token);
            }
        }

        public static void Remove(string token)
        {
            var rep = _repository.FirstOrDefault(x => Equals(x.Token, token));

            if (rep != null)
                _repository.TryTake(out rep);
        }

        public void Dispose()
        {
            StopGarbageCollector();
            lock (_locker)
            {
                Monitor.Pulse(_locker);
            }
        }
    }

    public class GlobalRepository
    {
        public GlobalRepository(string token)
        {
            Token = token;
            Fields = new Dictionary<string, object>();
        }

        public string Token { get; private set; }  
        public Dictionary<string, object> Fields { get; private set; }

        public DateTime LastUpdate { get; private set; }

        public void SetFieldValue(string field, object value)
        {
            if (!Fields.ContainsKey(field))
                Fields.Add(field, value);
            else
                Fields[field] = value;

            LastUpdate = DateTime.UtcNow;
        }

        public object GetFieldValue(string field)
        {
            object val;

            return Fields.TryGetValue(field, out val) ? val : null;
        }

        public void RemoveField(string field)
        {
            if (Fields.ContainsKey(field))
                Fields.Remove(field);
        }
    }
}