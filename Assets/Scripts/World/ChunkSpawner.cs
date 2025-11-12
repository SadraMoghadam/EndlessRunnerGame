using UnityEngine;
using System.Collections.Generic;

namespace World
{
    public class ChunkSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField] private float spawnDistanceAhead = 20f;
        [SerializeField] private float despawnDistanceBehind = 10f;
        [SerializeField] private int chunksToKeepAhead = 5;
        [SerializeField] private float playerZPosition = 0f;
        
        private ChunkPool _chunkPool;
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
        }
        
        private void Start()
        {
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
            
            _nextSpawnZ = chunk.EndZ;
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

