using System;
using System.Collections.Generic;

namespace SST.Pooling
{
    /// <summary>
    /// Reusable object pool for a single type <typeparamref name="T"/>. Instances are created on demand
    /// through a factory, handed out via <see cref="Get"/> and returned via <see cref="Release"/> to avoid
    /// repeated allocations. Optional lifecycle callbacks let you reset or configure instances as they move
    /// between the free and occupied sets. Not thread-safe.
    /// </summary>
    /// <typeparam name="T">Type of the pooled objects.</typeparam>
    public class Pool<T>
    {
        /// <summary>Number of instances currently available to be handed out.</summary>
        public int FreeCount => _freeObjects.Count;

        /// <summary>Number of instances currently in use (retrieved but not yet released).</summary>
        public int OccupiedCount => _occupiedObjects.Count;

        /// <summary>Instances available to be handed out.</summary>
        public IReadOnlyCollection<T> Free => _freeObjects;

        /// <summary>Instances currently in use.</summary>
        public IReadOnlyCollection<T> Occupied => _occupiedObjects;

        private readonly Func<T> _factory;
        private readonly Action<T> _actionOnGet;
        private readonly Action<T> _actionOnRelease;
        private readonly Action<T> _actionOnDiscard;

        private readonly List<T> _freeObjects = new();
        private readonly HashSet<T> _occupiedObjects = new();

        /// <summary>
        /// Creates a new pool.
        /// </summary>
        /// <param name="factory">Creates a new instance when the pool has no free objects to reuse.</param>
        /// <param name="actionOnGet">Invoked on an instance right before it is returned by <see cref="Get"/>.</param>
        /// <param name="actionOnRelease">Invoked on an instance when it is returned to the pool via <see cref="Release"/> (and on prewarmed instances).</param>
        /// <param name="actionOnDiscard">Invoked on an instance when it is permanently removed via <see cref="Discard"/> or <see cref="DiscardAll"/>; use it to free unmanaged resources.</param>
        public Pool(Func<T> factory, Action<T> actionOnGet = null, Action<T> actionOnRelease = null, Action<T> actionOnDiscard = null)
        {
            _factory = factory;
            _actionOnGet = actionOnGet;
            _actionOnRelease = actionOnRelease;
            _actionOnDiscard = actionOnDiscard;
        }

        /// <summary>
        /// Eagerly creates <paramref name="count"/> instances and adds them to the free set, so later
        /// <see cref="Get"/> calls reuse them instead of allocating. Each instance passes through the release callback.
        /// </summary>
        /// <param name="count">Number of instances to pre-allocate.</param>
        public void Prewarm(int count)
        {
            for (var i = 0; i < count; i++)
            {
                var instance = _factory.Invoke();
                _actionOnRelease?.Invoke(instance);
                _freeObjects.Add(instance);
            }
        }

        /// <summary>
        /// Retrieves an instance from the pool, reusing a free one when available or creating a new one via the factory.
        /// The returned instance is marked as occupied and must be returned with <see cref="Release"/> when no longer needed.
        /// </summary>
        /// <returns>A ready-to-use instance.</returns>
        public T Get()
        {
            T instance;

            if (_freeObjects.Count == 0)
            {
                instance = _factory.Invoke();
            }
            else
            {
                var lastIndex = _freeObjects.Count - 1;
                instance = _freeObjects[lastIndex];
                _freeObjects.RemoveAt(lastIndex);
            }

            _actionOnGet?.Invoke(instance);
            _occupiedObjects.Add(instance);

            return instance;
        }

        /// <summary>
        /// Returns an instance to the pool so it can be reused. Does nothing if the instance is not currently
        /// tracked as occupied by this pool (e.g. it was never handed out or has already been released).
        /// </summary>
        /// <param name="instance">The instance to return.</param>
        public void Release(T instance)
        {
            if (_occupiedObjects.Remove(instance))
            {
                _actionOnRelease?.Invoke(instance);
                _freeObjects.Add(instance);
            }
        }

        /// <summary>
        /// Permanently removes an instance from the pool (whether occupied or free) and runs the discard callback.
        /// Does nothing if the instance is not tracked by this pool.
        /// </summary>
        /// <param name="instance">The instance to discard.</param>
        public void Discard(T instance)
        {
            if (_occupiedObjects.Remove(instance))
            {
                _actionOnDiscard?.Invoke(instance);
                return;
            }

            if (_freeObjects.Remove(instance))
                _actionOnDiscard?.Invoke(instance);
        }

        /// <summary>
        /// Permanently removes every instance from the pool, running the discard callback on each one, and empties
        /// both the free and occupied sets.
        /// </summary>
        public void DiscardAll()
        {
            if (_actionOnDiscard != null)
            {
                foreach (var instance in _freeObjects)
                    _actionOnDiscard.Invoke(instance);

                foreach (var instance in _occupiedObjects)
                    _actionOnDiscard.Invoke(instance);
            }

            _freeObjects.Clear();
            _occupiedObjects.Clear();
        }
    }
}