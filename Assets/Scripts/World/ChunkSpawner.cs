using UnityEngine;
using System.Collections.Generic;
using Managers;

namespace World
{
    public class ChunkSpawner : MonoBehaviour
    {
        //[System.Serializable]
        //public class ChunkEntry
        //{
        //    public ChunkLayoutSO layout;
        //}

        [Header("Spawn Settings")]
        [SerializeField] private float spawnDistanceAhead = 20f;
        [SerializeField] private float despawnDistanceBehind = 10f;
        [SerializeField] private int chunksToKeepAhead = 5;
        [SerializeField] private float playerZPosition = 0f;

        [Header("Chunk Templates")]
        [Tooltip("List of chunk layouts (ScriptableObjects). Each layout carries its own difficulty setting.")]
        [SerializeField] private List<ChunkLayoutSO> chunkLayouts = new List<ChunkLayoutSO>();

        [Header("Scriptable Layout / Prefabs")]
        [Tooltip("Optional: default layout used if selection fails.")]
        [SerializeField] private ChunkLayoutSO defaultChunkLayout;
        [Tooltip("Prefab for moving obstacles (cell type = Moving).")]
        [SerializeField] private GameObject movingObstaclePrefab;
        [Tooltip("Prefab for static obstacles (cell type = Static).")]
        [SerializeField] private GameObject staticObstaclePrefab;
        [Tooltip("Prefab for jump obstacles (cell type = Jump). Optional - falls back to static.")]
        [SerializeField] private GameObject jumpObstaclePrefab;
        [Tooltip("Prefab for collectibles (cell type = Collectible).")]
        [SerializeField] private GameObject collectiblePrefab;

        private ChunkPool _chunkPool;
        private WorldManager worldManager;
        private List<WorldChunk> _activeChunks = new List<WorldChunk>();
        private float _nextSpawnZ;

        public float PlayerZPosition 
        { 
            get => playerZPosition; 
            set => playerZPosition = value; 
        }

        private void Awake()
        {
            _chunkPool = GetComponent<ChunkPool>();
            if (_chunkPool == null)
            {
                Debug.LogError("ChunkSpawner: ChunkPool component not found!");
            }

            if (worldManager == null)
            {
                Debug.LogWarning("ChunkSpawner: WorldManager not assigned and not found in scene. Lane positions might be unavailable.");
            }
        }

        private void Start()
        {
            if (worldManager == null)
            {
                worldManager = GameController.Instance?.WorldManager;
            }
            _nextSpawnZ = playerZPosition;
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

            // choose a layout based on difficulty distribution
            ChunkLayoutSO chosenLayout = ChooseLayoutByDifficulty();
            Debug.Log("Chosen level " + chosenLayout.name);

            // get a chunk instance from pool (we assume pool uses a single chunk prefab or handles instantiation)
            WorldChunk chunk = _chunkPool.GetChunk();
            if (chunk == null) return;

            float spawnZ;
            if (_activeChunks.Count > 0)
            {
                WorldChunk lastChunk = _activeChunks[_activeChunks.Count - 1];
                spawnZ = lastChunk.EndZ;
            }
            else
            {
                spawnZ = _nextSpawnZ;
            }
            
            chunk.Initialize(spawnZ);
            _activeChunks.Add(chunk);

            // If the chunk prefab has its own ChunkLayoutReference, prefer that. Otherwise use chosenLayout or fallback to default.
            ChunkLayoutSO layout = defaultChunkLayout;
            var layoutComponent = chunk.GetComponent<ChunkLayoutReference>();
            if (layoutComponent != null && layoutComponent.layout != null)
            {
                layout = layoutComponent.layout;
            }
            else if (chosenLayout != null)
            {
                layout = chosenLayout;
            }

            if (layout != null && worldManager != null)
            {
                ChunkGenerator.GenerateFromScriptable(
                    chunk,
                    worldManager,
                    layout,
                    movingObstaclePrefab,
                    staticObstaclePrefab,
                    jumpObstaclePrefab,
                    collectiblePrefab
                );
            }

            _nextSpawnZ = chunk.EndZ;
        }

        private ChunkLayoutSO ChooseLayoutByDifficulty()
        {
            if (chunkLayouts == null || chunkLayouts.Count == 0) return null;

            // Determine difficulty by fixed probabilities:
            // Easy50%, Medium30%, Hard15%, Extreme5%
            float r = Random.value;

            Difficulty chosenDifficulty;
            if (r < 0.50f) chosenDifficulty = Difficulty.Easy;
            else if (r < 0.80f) chosenDifficulty = Difficulty.Medium;
            else if (r < 0.95f) chosenDifficulty = Difficulty.Hard;
            else chosenDifficulty = Difficulty.Extreme;

            // pick a random layout from that difficulty (read difficulty from layout SO)
            var candidates = chunkLayouts.FindAll(e => e != null && e.difficulty == chosenDifficulty);
            if (candidates != null && candidates.Count > 0)
            {
                var entry = candidates[Random.Range(0, candidates.Count)];
                return entry;
            }

            // fallback strategies if no layouts for that difficulty:
            Difficulty[] fallbackOrder;
            switch (chosenDifficulty)
            {
                case Difficulty.Extreme: fallbackOrder = new[] { Difficulty.Hard, Difficulty.Medium, Difficulty.Easy }; break;
                case Difficulty.Hard: fallbackOrder = new[] { Difficulty.Medium, Difficulty.Easy, Difficulty.Extreme }; break;
                case Difficulty.Medium: fallbackOrder = new[] { Difficulty.Easy, Difficulty.Hard, Difficulty.Extreme }; break;
                default: fallbackOrder = new[] { Difficulty.Medium, Difficulty.Hard, Difficulty.Extreme }; break;
            }

            foreach (var d in fallbackOrder)
            {
                var list = chunkLayouts.FindAll(e => e != null && e.difficulty == d);
                if (list != null && list.Count > 0)
                    return list[Random.Range(0, list.Count)];
            }

            // last resort: any layout from chunkEntries
            var anyList = chunkLayouts.FindAll(e => e != null);
            if (anyList != null && anyList.Count > 0) return anyList[Random.Range(0, anyList.Count)];

            return null;
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
            _nextSpawnZ = playerZPosition;
        }
    }
}

