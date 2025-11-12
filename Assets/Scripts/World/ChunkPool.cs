using UnityEngine;
using System.Collections.Generic;

namespace World
{
    public class ChunkPool : MonoBehaviour
    {
        [Header("Pool Settings")]
        [SerializeField] private WorldChunk chunkPrefab;
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private int maxPoolSize = 50;
        
        private Queue<WorldChunk> availableChunks = new Queue<WorldChunk>();
        private List<WorldChunk> allChunks = new List<WorldChunk>();
        
        private void Awake()
        {
            if (chunkPrefab == null)
            {
                Debug.LogError("ChunkPool: Chunk prefab is not assigned!");
                return;
            }
            
            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewChunk();
            }
        }
        
        public WorldChunk GetChunk()
        {
            WorldChunk chunk;
            
            if (availableChunks.Count > 0)
            {
                chunk = availableChunks.Dequeue();
            }
            else
            {
                if (allChunks.Count < maxPoolSize)
                {
                    chunk = CreateNewChunk();
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
            availableChunks.Enqueue(chunk);
        }
        
        private WorldChunk CreateNewChunk()
        {
            WorldChunk chunk = Instantiate(chunkPrefab, transform);
            chunk.gameObject.SetActive(false);
            chunk.name = $"WorldChunk_{allChunks.Count}";
            allChunks.Add(chunk);
            availableChunks.Enqueue(chunk);
            return chunk;
        }
        
        public int GetActiveChunkCount()
        {
            return allChunks.Count - availableChunks.Count;
        }
    }
}

