using System;
using System.Collections.Generic;
using System.Linq;

namespace Kw.Common.DynamicResources
{
    /// <summary>
    /// Динамический пул разделяемых между потоками ресурсов.
    /// </summary>
    /// <typeparam name="T">Тип управляемого ресурса.</typeparam>
    /// <remarks>
    /// Динамический пул выделяет экземпляр ресурса на каждый поток.
    /// Выделение ресурса инициируется классом <see cref="DynamicResourceAllocator&lt;T&gt;"/> внутри блока using.
    /// Освобождение ресурса происходит при выходе из блока using.
    /// Если все созданные ресурсы распределены то при выделении ресурса создается новый экземпляр.
    /// </remarks>
    public static class DynamicResourcePool<T> where T : class, IDisposable
    {
        private static readonly object _guard = new object();
        private static readonly Dictionary<T, DynamicResourceAllocator<T>> _instances = new Dictionary<T, DynamicResourceAllocator<T>>();

        [ThreadStatic]
        private static T _resource;

        /// <summary>
        /// Количество ресурсов в пуле.
        /// </summary>
        public static int Capacity
        {
            get
            {
                lock (_guard)
                {
                    return _instances.Count;
                }
            }
        }

        /// <summary>
        /// Ссылка на экземпляр ресурса выделенный текущему потоку.
        /// </summary>
        public static T AllocatedResource
        {
            get
            {
                lock (_guard)
                {
                    return _resource;
                }
            }
        }

        /// <summary>
        /// Выделяет свободный или создает новый экземпляр ресурса.
        /// </summary>
        /// <param name="allocator">Объект-аллокатор.</param>
        internal static void AllocateResource(DynamicResourceAllocator<T> allocator)
        {
            if (allocator == null) throw new ArgumentNullException(nameof(allocator));

            lock (_guard)
            {
                if (null != _resource)
                    throw new InvalidOperationException($@"Resource '{typeof (T).FullName}' has already been allocated for the current thread.");

                var candidate = _instances.Keys.FirstOrDefault(k => _instances[k] == null);

                if (null != candidate)
                {
                    if (!allocator.VerifyResource(candidate))
                    {
                        _instances.Remove(candidate);
                        Runtime.Release(ref candidate);
                    }
                }

                _resource = candidate ?? allocator.CreateInstance();
                
                if (null != candidate)
                {
                    _instances[candidate] = allocator;
                }
                else
                {
                    _instances.Add(_resource, allocator);
                }
            }
        }

        /// <summary>
        /// Освобождает ресурс и возвращает его в пул.
        /// </summary>
        internal static void FreeResource()
        {
            if (null == _resource)
                throw new InvalidOperationException($@"Resource '{typeof(T).FullName}' hasn't been allocated for the current thread.");

            lock (_guard)
            {
                _instances[_resource] = null;

                _resource = null;
            }
        }
    }
}

