using UnityEngine;
using System.Collections.Generic;
using Managers;

namespace World
{
    public class WorldChunk : MonoBehaviour
    {
        [Header("Chunk Settings")]
        [SerializeField] private float chunkLength = 20f;

        private Transform chunkStartPoint;
        private Transform chunkEndPoint;
        private readonly List<IWorldObject> _worldObjects = new List<IWorldObject>();
        private float _currentZPosition;
        private bool _isActive = false;
        private Dictionary<Transform, Vector3> _originalObstaclePositions = new Dictionary<Transform, Vector3>();
        private GameObject _sourcePrefab;
        
        public GameObject SourcePrefab
        {
            get => _sourcePrefab;
            set => _sourcePrefab = value;
        }
        
        public float ChunkLength => chunkLength;
        public float StartZ => _currentZPosition;
        public float EndZ => _currentZPosition + chunkLength;
        public bool IsActive => _isActive;
        
        private void Awake()
        {
            if (chunkStartPoint == null)
            {
                GameObject startObj = new GameObject("ChunkStart");
                startObj.transform.SetParent(transform);
                startObj.transform.localPosition = Vector3.zero;
                chunkStartPoint = startObj.transform;
            }
            
            if (chunkEndPoint == null)
            {
                GameObject endObj = new GameObject("ChunkEnd");
                endObj.transform.SetParent(transform);
                endObj.transform.localPosition = new Vector3(0, 0, chunkLength);
                chunkEndPoint = endObj.transform;
            }
        }
        
        public void Initialize(float zPosition)
        {
            _currentZPosition = zPosition;
            transform.position = new Vector3(0, 0, zPosition);
            _isActive = true;
            gameObject.SetActive(true);
            
            // Store original positions of all obstacles (including dynamic ones that move)
            StoreOriginalObstaclePositions();
        }
        
        private void StoreOriginalObstaclePositions()
        {
            _originalObstaclePositions.Clear();
            WorldObstacle[] obstacles = GetComponentsInChildren<WorldObstacle>(true);
            foreach (WorldObstacle obstacle in obstacles)
            {
                if (obstacle != null && obstacle.transform != null)
                {
                    _originalObstaclePositions[obstacle.transform] = obstacle.transform.localPosition;
                }
            }
        }
        
        public void MoveChunk(float deltaMovement)
        {
            if (!_isActive) return;
            
            _currentZPosition -= deltaMovement;
            transform.position = new Vector3(transform.position.x, transform.position.y, _currentZPosition);

            var objectsCopy = _worldObjects.ToArray();
            foreach (var worldObject in objectsCopy)
            {
                if (worldObject == null) continue;

                var comp = worldObject as Component;
                if (comp != null)
                {
                    if (comp.transform.IsChildOf(transform))
                        continue;
                }
            }
        }
        
        public void AddWorldObject(IWorldObject worldObject)
        {
            if (!_worldObjects.Contains(worldObject))
            {
                _worldObjects.Add(worldObject);
            }
        }
        
        public void RemoveWorldObject(IWorldObject worldObject)
        {
            _worldObjects.Remove(worldObject);
        }
        
        public bool HasPassedPlayer(float playerZPosition, float despawnOffset = 5f)
        {
            return EndZ < playerZPosition - despawnOffset;
        }
        
        public void ResetChunk()
        {
            _isActive = false;

            var objectsCopy = _worldObjects.ToArray();

            _worldObjects.Clear();
            
            // Reset all obstacles to their original positions
            ResetObstaclePositions();
            
            gameObject.SetActive(false);
        }
        
        private void ResetObstaclePositions()
        {
            foreach (var kvp in _originalObstaclePositions)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.localPosition = kvp.Value;
                }
            }
        }
        
        private void OnDrawGizmos()
        {
            if (chunkStartPoint != null && chunkEndPoint != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(
                    transform.position + new Vector3(0, 0, chunkLength / 2),
                    new Vector3(10, 1, chunkLength)
                );
                
                Gizmos.color = Color.yellow;
                Vector3 start = transform.position;
                Vector3 end = transform.position + new Vector3(0, 0, chunkLength);
                
                WorldManager worldManager = GameController.Instance.WorldManager;
                Gizmos.DrawLine(start + new Vector3(worldManager.GetLaneXPosition(LaneNumber.Left), 0, 0), end + new Vector3(worldManager.GetLaneXPosition(LaneNumber.Left), 0, 0));
                Gizmos.DrawLine(start + new Vector3(worldManager.GetLaneXPosition(LaneNumber.Center), 0, 0), end + new Vector3(worldManager.GetLaneXPosition(LaneNumber.Center), 0, 0));
                Gizmos.DrawLine(start + new Vector3(worldManager.GetLaneXPosition(LaneNumber.Right), 0, 0), end + new Vector3(worldManager.GetLaneXPosition(LaneNumber.Right), 0, 0));
            }
        }
    }
}

