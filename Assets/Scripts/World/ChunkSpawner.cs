using UnityEngine;
using System.Collections.Generic;
using Managers;

namespace World
{
    public class ChunkSpawner : MonoBehaviour
    {

        [Header("Spawn Settings")]
        [SerializeField] private float spawnDistanceAhead = 20f;
        [SerializeField] private float despawnDistanceBehind = 10f;
        [SerializeField] private int chunksToKeepAhead = 2;
        [SerializeField] private float playerZPosition = 0f;

        [SerializeField] private List<ChunkLayoutSO> chunkLayouts = new List<ChunkLayoutSO>();
        [SerializeField] private ChunkLayoutSO defaultChunkLayout;

        private ChunkPool _chunkPool;
        private WorldManager worldManager;
        private List<WorldChunk> _activeChunks = new List<WorldChunk>();

        private void Awake()
        {
            _chunkPool = GetComponent<ChunkPool>();
            if (_chunkPool == null)
            {
                Debug.LogError("ChunkSpawner: ChunkPool component not found!");
            }
        }

        private void Start()
        {
            if (worldManager == null)
            {
                worldManager = GameController.Instance?.WorldManager;
            }
            SpawnInitialChunks();
        }

        private void Update()
        {
            while (_activeChunks.Count < chunksToKeepAhead || 
                   (_activeChunks.Count > 0 && _activeChunks[_activeChunks.Count - 1].EndZ < playerZPosition + spawnDistanceAhead))
            {
                SpawnChunk();
            }

            DespawnPassedChunks();
        }

        private void SpawnInitialChunks()
        {
            for (int i = 0; i < chunksToKeepAhead; i++)
            {
                SpawnChunk();
            }
        }

        private void SpawnChunk()
        {
            if (_chunkPool == null) return;

            ChunkLayoutSO chosenLayout = ChooseRandomLayout();
            if (chosenLayout == null)
            {
                Debug.LogWarning("ChunkSpawner: No layout available to spawn chunk!");
                return;
            }

            GameObject chunkPrefab = chosenLayout.chunkPrefab;
            if (chunkPrefab == null)
            {
                Debug.LogWarning($"ChunkSpawner: Layout {chosenLayout.name} has no chunkPrefab assigned!");
                chunkPrefab = defaultChunkLayout != null ? defaultChunkLayout.chunkPrefab : null;
            }

            WorldChunk chunk = _chunkPool.GetChunk(chunkPrefab);
            if (chunk == null) return;

            float spawnZ = 0;
            if (_activeChunks.Count > 0)
            {
                WorldChunk lastChunk = _activeChunks[_activeChunks.Count - 1];
                spawnZ = lastChunk.EndZ;
            }
            
            chunk.Initialize(spawnZ);
            _activeChunks.Add(chunk);
        }

        private ChunkLayoutSO ChooseRandomLayout()
        {
            // Filter out null layouts and layouts without prefabs
            var validLayouts = chunkLayouts.FindAll(e => e != null && e.chunkPrefab != null);
            
            if (validLayouts == null || validLayouts.Count == 0)
            {
                // Fallback to default if it has a prefab
                if (defaultChunkLayout != null && defaultChunkLayout.chunkPrefab != null)
                {
                    return defaultChunkLayout;
                }
                return null;
            }

            // Randomly select from valid layouts
            return validLayouts[Random.Range(0, validLayouts.Count)];
        }

        private void DespawnPassedChunks()
        {
            for (int i = _activeChunks.Count - 1; i >= 0; i--)
            {
                if (_activeChunks[i] == null || _activeChunks[i].HasPassedPlayer(playerZPosition, despawnDistanceBehind))
                {
                    WorldChunk chunkToRemove = _activeChunks[i];
                    _activeChunks.RemoveAt(i);
                    
                    if (_chunkPool != null && chunkToRemove != null)
                    {
                        _chunkPool.ReturnChunk(chunkToRemove);
                    }
                }
            }
        }
        
        public List<WorldChunk> GetActiveChunks()
        {
            return new List<WorldChunk>(_activeChunks);
        }
        
        public void DespawnAllChunks()
        {
            foreach (var chunk in _activeChunks)
            {
                if (chunk != null && _chunkPool != null)
                {
                    _chunkPool.ReturnChunk(chunk);
                }
            }
            _activeChunks.Clear();
        }
    }
}

