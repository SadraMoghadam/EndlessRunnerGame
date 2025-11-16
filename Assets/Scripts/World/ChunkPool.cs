using UnityEngine;
using System.Collections.Generic;

namespace World
{
    public class ChunkPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [Tooltip("Assign a single WorldChunk prefab. The pool will instantiate chunks from this prefab.")]
        [SerializeField] private WorldChunk chunkPrefab;
        [Tooltip("Initial number of instances to create.")]
        [SerializeField] private int initialPoolSize = 5;
        [SerializeField] private int maxPoolSize = 50;

        private Queue<WorldChunk> available = new Queue<WorldChunk>();
        private List<WorldChunk> allChunks = new List<WorldChunk>();

        private void Awake()
        {
            if (chunkPrefab == null)
            {
                Debug.LogError("ChunkPool: No chunk prefab assigned! Please assign a WorldChunk prefab.");
                return;
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewChunk();
            }
        }

        public WorldChunk GetChunk()
        {
            WorldChunk chunk = null;
            if (available.Count > 0)
            {
                chunk = available.Dequeue();
                if (chunk != null) chunk.gameObject.SetActive(true);
            }
            else
            {
                if (allChunks.Count < maxPoolSize)
                {
                    chunk = CreateNewChunk();
                    if (chunk != null) chunk.gameObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("ChunkPool: Max pool size reached! Consider increasing maxPoolSize.");
                    return null;
                }
            }

            return chunk;
        }

        public void ReturnChunk(WorldChunk chunk)
        {
            if (chunk == null) return;

            chunk.ResetChunk();
            chunk.gameObject.SetActive(false);
            available.Enqueue(chunk);
        }

        private WorldChunk CreateNewChunk()
        {
            if (chunkPrefab == null) return null;

            WorldChunk chunk = Instantiate(chunkPrefab, transform);
            chunk.gameObject.SetActive(false);
            chunk.name = $"{chunkPrefab.name}_Instance_{allChunks.Count}";
            allChunks.Add(chunk);
            available.Enqueue(chunk);
            return chunk;
        }

        public int GetActiveChunkCount()
        {
            return allChunks.Count - available.Count;
        }
    }
}

