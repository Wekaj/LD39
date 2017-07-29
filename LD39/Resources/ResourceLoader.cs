using System;
using System.Collections.Generic;

namespace LD39.Resources
{
    internal abstract class ResourceLoader<TID, TResource> : IDisposable
        where TResource : IDisposable
    {
        #region IDisposable Support
        private bool _disposed = false;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    foreach (TResource resource in _resources.Values)
                        resource.Dispose();
                }

                _resources.Clear();

                _disposed = true;
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
        }
        #endregion

        protected readonly Dictionary<TID, TResource> _resources = new Dictionary<TID, TResource>();

        public TResource this[TID id] => _resources[id];

        public TResource Load(TID id, string filename)
        {
            TResource resource = Load(filename);
            _resources.Add(id, resource);
            return resource;
        }

        protected abstract TResource Load(string filename);
    }
}
