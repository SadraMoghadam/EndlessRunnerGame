using UnityEngine;
using System.Collections.Generic;

namespace World
{
    public class ChunkPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [Tooltip("Initial number of instances to create per prefab.")]
        [SerializeField] private int initialPoolSize = 5;
        [SerializeField] private int maxPoolSize = 50;

        private class PoolData
        {
            public Queue<WorldChunk> available = new Queue<WorldChunk>();
            public List<WorldChunk> allChunks = new List<WorldChunk>();
        }

        private Dictionary<GameObject, PoolData> _pools = new Dictionary<GameObject, PoolData>();

        public WorldChunk GetChunk(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("ChunkPool: Cannot get chunk with null prefab!");
                return null;
            }

            // Get or create pool for this prefab
            if (!_pools.ContainsKey(prefab))
            {
                _pools[prefab] = new PoolData();
            }

            PoolData pool = _pools[prefab];
            WorldChunk chunk = null;

            // Try to get from available pool
            if (pool.available.Count > 0)
            {
                chunk = pool.available.Dequeue();
                if (chunk != null) chunk.gameObject.SetActive(true);
            }
            else
            {
                // Create new chunk if under max size
                if (pool.allChunks.Count < maxPoolSize)
                {
                    chunk = CreateNewChunk(prefab, pool);
                    if (chunk != null) chunk.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning($"ChunkPool: Max pool size reached for prefab {prefab.name}! Consider increasing maxPoolSize.");
                    return null;
                }
            }

            return chunk;
        }

        public void ReturnChunk(WorldChunk chunk)
        {
            if (chunk == null) return;

            // Find which pool this chunk belongs to by checking its ChunkLayoutReference
            GameObject sourcePrefab = GetSourcePrefab(chunk);
            if (sourcePrefab == null)
            {
                Debug.LogWarning("ChunkPool: Cannot return chunk - source prefab not found!");
                return;
            }

            if (!_pools.ContainsKey(sourcePrefab))
            {
                Debug.LogWarning($"ChunkPool: Pool not found for prefab {sourcePrefab.name}!");
                return;
            }

            PoolData pool = _pools[sourcePrefab];
            chunk.ResetChunk();
            chunk.gameObject.SetActive(false);
            pool.available.Enqueue(chunk);
        }

        private WorldChunk CreateNewChunk(GameObject prefab, PoolData pool)
        {
            if (prefab == null) return null;

            WorldChunk chunkComponent = prefab.GetComponent<WorldChunk>();
            if (chunkComponent == null)
            {
                Debug.LogError($"ChunkPool: Prefab {prefab.name} does not have a WorldChunk component!");
                return null;
            }

            WorldChunk chunk = Instantiate(chunkComponent, transform);
            chunk.gameObject.SetActive(false);
            chunk.name = $"{prefab.name}_Instance_{pool.allChunks.Count}";
            pool.allChunks.Add(chunk);
            return chunk;
        }

        private GameObject GetSourcePrefab(WorldChunk chunk)
        {
            // Try to get the prefab from ChunkLayoutReference
            var layoutRef = chunk.GetComponent<ChunkLayoutReference>();
            if (layoutRef != null && layoutRef.layout != null && layoutRef.layout.chunkPrefab != null)
            {
                return layoutRef.layout.chunkPrefab;
            }

            // Fallback: try to find the prefab by matching name (less reliable)
            // This is a fallback in case ChunkLayoutReference is not set
            string chunkName = chunk.name;
            if (chunkName.Contains("_Instance_"))
            {
                string prefabName = chunkName.Substring(0, chunkName.IndexOf("_Instance_"));
                // Try to find in pools
                foreach (var kvp in _pools)
                {
                    if (kvp.Key.name == prefabName)
                    {
                        return kvp.Key;
                    }
                }
            }

            return null;
        }

        public int GetActiveChunkCount()
        {
            int count = 0;
            foreach (var pool in _pools.Values)
            {
                count += pool.allChunks.Count - pool.available.Count;
            }
            return count;
        }
    }
}

