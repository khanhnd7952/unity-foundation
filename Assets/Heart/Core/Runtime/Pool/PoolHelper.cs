using System.Linq;
using UnityEngine;

namespace Pancake
{
    public static partial class PoolHelper
    {
        public static void Populate(this GameObject prefab, int count, bool persistent = false) { Pool.GetPoolByPrefab(prefab, persistent: persistent).Populate(count); }

        /// <summary>
        /// Clear all instance inside pool
        /// </summary>
        /// <param name="prefab"></param>
        /// <param name="isDestroy"></param>
        public static void Clear(this GameObject prefab, bool isDestroy = true) { Pool.GetPoolByPrefab(prefab, false)?.Clear(isDestroy); }

        /// <summary>
        /// Return all outside instance to pool
        /// </summary>
        /// <param name="prefab"></param>
        public static void ReturnAll(this GameObject prefab) { Pool.GetPoolByPrefab(prefab, false)?.ReturnAll(); }

        public static GameObject Request(this GameObject prefab) { return Pool.GetPoolByPrefab(prefab).Request(); }

        public static GameObject Request(this GameObject prefab, Transform parent) { return Pool.GetPoolByPrefab(prefab).Request(parent); }

        public static GameObject Request(this GameObject prefab, Transform parent, bool worldPositionStays)
        {
            return Pool.GetPoolByPrefab(prefab).Request(parent, worldPositionStays);
        }

        public static GameObject Request(this GameObject prefab, Vector3 position, Quaternion rotation)
        {
            return Pool.GetPoolByPrefab(prefab).Request(position, rotation);
        }

        public static GameObject Request(this GameObject prefab, Vector3 position, Quaternion rotation, Transform parent)
        {
            return Pool.GetPoolByPrefab(prefab).Request(position, rotation, parent);
        }

        public static T Request<T>(this GameObject prefab) where T : Component { return prefab.Request().GetComponent<T>(); }

        public static T Request<T>(this GameObject prefab, Transform parent) where T : Component { return prefab.Request(parent).GetComponent<T>(); }

        public static T Request<T>(this GameObject prefab, Transform parent, bool worldPositionStays) where T : Component
        {
            return prefab.Request(parent, worldPositionStays).GetComponent<T>();
        }

        public static T Request<T>(this GameObject prefab, Vector3 position, Quaternion rotation) where T : Component
        {
            return prefab.Request(position, rotation).GetComponent<T>();
        }

        public static T Request<T>(this GameObject prefab, Vector3 position, Quaternion rotation, Transform parent) where T : Component
        {
            return prefab.Request(position, rotation, parent).GetComponent<T>();
        }

        public static void Return(this GameObject instance)
        {
            bool isPooled = Pool.GetPoolByInstance(instance, out var pool);
            if (isPooled) pool.Return(instance);
            else Object.Destroy(instance);
        }

        public static void ClearAllPool()
        {
            foreach (var lookup in Pool.PrefabLookup.ToList())
            {
                lookup.Value.Clear();
            }
        }

        public static void ReturnAllPool()
        {
            foreach (var lookup in Pool.PrefabLookup.ToList())
            {
                lookup.Value.ReturnAll();
            }
        }
    }
}