using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Superglazka.Core
{
    public class ObjectPool<T> where T : Object
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly UnityEngine.Pool.ObjectPool<T> _pool;

        public ObjectPool(T prefab, Transform parent, int defaultCapacity = 10, int maxSize = 100)
        {
            _prefab = prefab;
            _parent = parent;
            _pool = new ObjectPool<T>(
                CreateFunc,
                OnGet,
                OnRelease,
                OnDestroy,
                collectionCheck: Application.isEditor,
                defaultCapacity: defaultCapacity,
                maxSize: maxSize
            );
        }

        private T CreateFunc()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            var go = obj as GameObject ?? (obj as Component)?.gameObject;
            go?.SetActive(false);
            return obj;
        }

        private void OnGet(T obj)
        {
            var go = obj as GameObject ?? (obj as Component)?.gameObject;
            go?.SetActive(true);
        }
        private void OnRelease(T obj)
        {
            var go = obj as GameObject ?? (obj as Component)?.gameObject;
            go?.SetActive(false);
        }
        private void OnDestroy(T obj) => Object.Destroy(obj);

        public T Get() => _pool.Get();
        public void Release(T obj) => _pool.Release(obj);
        public void Clear() => _pool.Clear();
    }
}
