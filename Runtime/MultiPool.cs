using System;
using System.Collections.Generic;

namespace SST.Pooling
{
    /// <summary>
    /// Manages a set of <see cref="Pool{T}"/> instances keyed by <typeparamref name="TKey"/>, so a single
    /// object can pool several kinds of <typeparamref name="TValue"/> (e.g. different prefabs or variants)
    /// under one API. Each key must be registered with its own factory before use. Not thread-safe.
    /// </summary>
    /// <typeparam name="TKey">Key identifying an individual pool.</typeparam>
    /// <typeparam name="TValue">Type of the pooled objects.</typeparam>
    public class MultiPool<TKey, TValue>
    {
        private readonly Dictionary<TKey, Pool<TValue>> _pools = new();

        /// <summary>Returns whether a pool has been registered for the given <paramref name="key"/>.</summary>
        /// <param name="key">Key to check.</param>
        public bool HasFactory(TKey key) => _pools.ContainsKey(key);

        /// <summary>
        /// Registers a pool for <paramref name="key"/>. Has no effect if the key is already registered.
        /// </summary>
        /// <param name="key">Key the new pool is bound to.</param>
        /// <param name="factory">Creates a new instance when the pool has no free objects to reuse.</param>
        /// <param name="actionOnGet">Invoked on an instance right before it is handed out.</param>
        /// <param name="actionOnRelease">Invoked on an instance when it is returned to the pool.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="factory"/> is null.</exception>
        public void RegisterFactory(TKey key, Func<TValue> factory, Action<TValue> actionOnGet = null, Action<TValue> actionOnRelease = null)
        {
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _pools.TryAdd(key, new Pool<TValue>(factory, actionOnGet, actionOnRelease));
        }

        /// <summary>
        /// Pre-allocates <paramref name="count"/> instances for the pool bound to <paramref name="key"/>.
        /// Does nothing if the key is not registered.
        /// </summary>
        /// <param name="key">Key of the pool to prewarm.</param>
        /// <param name="count">Number of instances to pre-allocate.</param>
        public void Prewarm(TKey key, int count)
        {
            if (_pools.TryGetValue(key, out var pool))
                pool.Prewarm(count);
        }

        /// <summary>
        /// Retrieves an instance from the pool bound to <paramref name="key"/>.
        /// </summary>
        /// <param name="key">Key of the pool to draw from.</param>
        /// <returns>A ready-to-use instance.</returns>
        /// <exception cref="KeyNotFoundException">Thrown when no factory is registered for <paramref name="key"/>.</exception>
        public TValue Get(TKey key)
        {
            if (_pools.TryGetValue(key, out var pool))
                return pool.Get();

            throw new KeyNotFoundException($"[MultiPool] Cannot get object: factory for key '{key}' is not registered!");
        }

        /// <summary>
        /// Returns an instance to the pool bound to <paramref name="key"/>. Does nothing if the key is not registered.
        /// </summary>
        /// <param name="key">Key of the pool the instance belongs to.</param>
        /// <param name="instance">The instance to return.</param>
        public void Release(TKey key, TValue instance)
        {
            if (_pools.TryGetValue(key, out var pool))
                pool.Release(instance);
        }

        /// <summary>
        /// Permanently removes a single instance from the pool bound to <paramref name="key"/>.
        /// Does nothing if the key is not registered.
        /// </summary>
        /// <param name="key">Key of the pool the instance belongs to.</param>
        /// <param name="instance">The instance to discard.</param>
        public void Discard(TKey key, TValue instance)
        {
            if (_pools.TryGetValue(key, out var pool))
                pool.Discard(instance);
        }

        /// <summary>
        /// Permanently removes every instance from the pool bound to <paramref name="key"/>.
        /// Does nothing if the key is not registered.
        /// </summary>
        /// <param name="key">Key of the pool to clear.</param>
        public void DiscardAll(TKey key)
        {
            if (_pools.TryGetValue(key, out var pool))
                pool.DiscardAll();
        }
    }
}