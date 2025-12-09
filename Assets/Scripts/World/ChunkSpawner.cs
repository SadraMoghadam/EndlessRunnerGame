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
        [SerializeField] private int chunksToKeepAhead = 5;
        [SerializeField] private float playerZPosition = 0f;

        [Header("Moving Obstacle Settings")]
        [Tooltip("Distance from player at which moving (dynamic) obstacles from chunk layouts will be instantiated.")]
        [SerializeField] private float movingSpawnDistance = 20f;
        [Tooltip("Distance from player at which a spawned moving obstacle will be activated (taken out of dormant state)")]
        [SerializeField] private float movingActivationDistance = 10f;

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

        [Header("Object Configs")]
        [Tooltip("ObjectConfigSO for static obstacles. If set, obstacles will be instantiated from random ObjectData in this config.")]
        [SerializeField] private ObjectConfigSO staticObstacleConfig;
        [Tooltip("ObjectConfigSO for jump obstacles. If set, obstacles will be instantiated from random ObjectData in this config. Falls back to staticObstacleConfig if not set.")]
        [SerializeField] private ObjectConfigSO jumpObstacleConfig;

        private ChunkPool _chunkPool;
        private WorldManager worldManager;
        private List<WorldChunk> _activeChunks = new List<WorldChunk>();
        private float _nextSpawnZ;

        // track which moving cells have been spawned to avoid duplicates
        private readonly HashSet<string> _spawnedMovingCells = new HashSet<string>();

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

            // spawn moving obstacles from chunk layouts when they approach the player
            SpawnMovingObstacles();

            DespawnPassedChunks();

            // ensure pooled moving obstacles that have passed the player are returned/deactivated
            var pool = DynamicObstaclePool.Instance;
            if (pool != null)
            {
                pool.DeactivatePassedMovingObstacles(playerZPosition, despawnDistanceBehind);
            }
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
            Debug.Log("Chosen level " + (chosenLayout != null ? chosenLayout.name : "null"));

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

            // Ensure the chunk's ChunkLayoutReference is populated so later code can read it (prevent null when reading active chunks)
            if (layout != null)
            {
                if (layoutComponent == null)
                {
                    layoutComponent = chunk.gameObject.AddComponent<ChunkLayoutReference>();
                }
                layoutComponent.layout = layout;
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
                    collectiblePrefab,
                    staticObstacleConfig,
                    jumpObstacleConfig
                );
            }

            _nextSpawnZ = chunk.EndZ;
        }

        private void SpawnMovingObstacles()
        {
            if (movingObstaclePrefab == null || worldManager == null) return;
            var player = GameController.Instance?.PlayerController;
            if (player == null) return;

            var pool = DynamicObstaclePool.Instance;
            var dynamicRoot = pool.Root;

            for (int i = 0; i < _activeChunks.Count; i++)
            {
                var chunk = _activeChunks[i];
                if (chunk == null) continue;

                var layoutComp = chunk.GetComponent<ChunkLayoutReference>();
                if (layoutComp == null || layoutComp.layout == null) continue;

                var layout = layoutComp.layout;
                float cs = layout.cellSize > 0f ? layout.cellSize : 5f;
                int segments = Mathf.Max(0, layout.segments);
                int lanes = Mathf.Max(1, layout.lanes);

                for (int seg = 0; seg < segments; seg++)
                {
                    for (int ln = 0; ln < lanes; ln++)
                    {
                        if (layout.GetCell(seg, ln) != ChunkLayoutSO.CellType.Moving) continue;

                        float worldZ = chunk.StartZ + (seg + 0.5f) * cs;

                        // only consider cells ahead of player and within movingSpawnDistance
                        if (worldZ < player.transform.position.z) continue;
                        if (worldZ > player.transform.position.z + movingSpawnDistance) continue;

                        string key = chunk.GetInstanceID() + "_" + seg + "_" + ln;
                        if (_spawnedMovingCells.Contains(key)) continue;

                        // spawn moving obstacle as a WorldObstacle instance (prefab should have WorldObstacle and be configured as moving)
                        LaneNumber lane = ln == 0 ? LaneNumber.Left : (ln == 1 ? LaneNumber.Center : LaneNumber.Right);
                        float x = worldManager.GetLaneXPosition(lane);
                        Vector3 pos = new Vector3(x, 0f, worldZ);

                        WorldObstacle spawned = null;

                        if (pool != null)
                        {
                            spawned = pool.Get();
                            if (spawned != null)
                            {
                                spawned.transform.position = pos;
                                spawned.transform.rotation = Quaternion.identity;
                                // keep it dormant until close to player
                                spawned.SetDormant(true);
                            }
                        }

                        if (spawned == null)
                        {
                            var inst = Object.Instantiate(movingObstaclePrefab, pos, Quaternion.identity, dynamicRoot);
                            if (inst != null)
                            {
                                spawned = inst.GetComponent<WorldObstacle>();
                                if (spawned != null)
                                {
                                    spawned.SetDormant(true);
                                }
                            }
                        }

                        if (spawned != null)
                        {
                            // if within activation distance, activate immediately
                            float distanceToPlayer = spawned.transform.position.z - player.transform.position.z;
                            if (distanceToPlayer <= movingActivationDistance)
                            {
                                spawned.SetDormant(false);
                            }

                            _spawnedMovingCells.Add(key);
                        }
                    }
                }
            }
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
                    
                    // Clean up spawned moving cells for this chunk to prevent HashSet from growing indefinitely
                    if (chunkToRemove != null)
                    {
                        int chunkInstanceId = chunkToRemove.GetInstanceID();
                        var keysToRemove = new List<string>();
                        foreach (var key in _spawnedMovingCells)
                        {
                            if (key.StartsWith(chunkInstanceId + "_"))
                            {
                                keysToRemove.Add(key);
                            }
                        }
                        foreach (var key in keysToRemove)
                        {
                            _spawnedMovingCells.Remove(key);
                        }
                    }
                    
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
            _spawnedMovingCells.Clear();
        }
    }
}

