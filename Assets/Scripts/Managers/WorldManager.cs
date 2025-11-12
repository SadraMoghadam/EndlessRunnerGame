using UnityEngine;
using World;

namespace Managers
{
    public class WorldManager : MonoBehaviour
    {
        [Header("Component References")]
        [SerializeField] private WorldMover worldMover;
        [SerializeField] private ChunkSpawner chunkSpawner;
        [SerializeField] private ChunkPool chunkPool;
        
        [Header("Player Reference")]
        [SerializeField] private Transform playerTransform;
        [SerializeField] private float playerZOffset = 0f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        private void Awake()
        {
            if (worldMover == null)
                worldMover = GetComponent<WorldMover>();
            
            if (chunkSpawner == null)
                chunkSpawner = GetComponent<ChunkSpawner>();
            
            if (chunkPool == null)
                chunkPool = GetComponent<ChunkPool>();
            
            if (worldMover == null)
                Debug.LogError("WorldManager: WorldMover component not found!");
            
            if (chunkSpawner == null)
                Debug.LogError("WorldManager: ChunkSpawner component not found!");
            
            if (chunkPool == null)
                Debug.LogError("WorldManager: ChunkPool component not found!");
        }
        
        private void Start()
        {
            if (playerTransform != null)
            {
                UpdatePlayerPosition();
            }
        }
        
        private void Update()
        {
            if (playerTransform != null)
            {
                UpdatePlayerPosition();
            }
            
            MoveWorld();
        }
        
        private void UpdatePlayerPosition()
        {
            float playerZ = playerTransform.position.z + playerZOffset;
            if (chunkSpawner != null)
            {
                chunkSpawner.PlayerZPosition = playerZ;
            }
        }
        
        private void MoveWorld()
        {
            if (worldMover == null || chunkSpawner == null) return;
            
            float movementDelta = worldMover.GetMovementDelta();
            
            var activeChunks = chunkSpawner.GetActiveChunks();
            foreach (var chunk in activeChunks)
            {
                if (chunk != null && chunk.IsActive)
                {
                    chunk.MoveChunk(movementDelta);
                }
            }
        }
        
        public void SetWorldSpeed(float speed)
        {
            if (worldMover != null)
            {
                worldMover.SetSpeed(speed);
            }
        }
        
        public void SetSpeedMultiplier(float multiplier)
        {
            if (worldMover != null)
            {
                worldMover.SpeedMultiplier = multiplier;
            }
        }
        
        public void PauseWorld()
        {
            if (worldMover != null)
            {
                worldMover.Pause();
            }
        }
        
        public void ResumeWorld()
        {
            if (worldMover != null)
            {
                worldMover.Resume();
            }
        }
        
        public void ResetWorld()
        {
            if (chunkSpawner != null)
            {
                chunkSpawner.DespawnAllChunks();
            }
            
            if (worldMover != null)
            {
                worldMover.ResetSpeed();
            }
        }
        
        public float GetCurrentSpeed()
        {
            return worldMover != null ? worldMover.CurrentSpeed : 0f;
        }
        
        public float GetTotalDistance()
        {
            return worldMover != null ? worldMover.TotalDistanceTraveled : 0f;
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Box("World Manager Debug");
            GUILayout.Label($"Speed: {GetCurrentSpeed():F1} m/s");
            GUILayout.Label($"Distance: {GetTotalDistance():F1} m");
            
            if (chunkSpawner != null)
            {
                var chunks = chunkSpawner.GetActiveChunks();
                GUILayout.Label($"Active Chunks: {chunks.Count}");
            }
            
            if (chunkPool != null)
            {
                GUILayout.Label($"Pool Active: {chunkPool.GetActiveChunkCount()}");
            }
            
            GUILayout.EndArea();
        }
    }
}

