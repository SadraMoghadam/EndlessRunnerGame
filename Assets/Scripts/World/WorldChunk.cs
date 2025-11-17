using UnityEngine;
using System.Collections.Generic;
using Managers;

namespace World
{
    public class WorldChunk : MonoBehaviour
    {
        [Header("Chunk Settings")]
        [SerializeField] private float chunkLength = 20f;
        [SerializeField] private Transform chunkStartPoint;
        [SerializeField] private Transform chunkEndPoint;
        [SerializeField] private Difficulty chunkLevel;
        

        private readonly List<IWorldObject> _worldObjects = new List<IWorldObject>();
        private float _currentZPosition;
        private bool _isActive = false;
        
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
        }
        
        public void MoveChunk(float deltaMovement)
        {
            if (!_isActive) return;
            
            _currentZPosition -= deltaMovement;
            transform.position = new Vector3(transform.position.x, transform.position.y, _currentZPosition);
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

            // Make a copy because calling OnDespawn may remove the object from the original list,
            // which would modify the collection during enumeration and throw InvalidOperationException.
            var objectsCopy = _worldObjects.ToArray();

            foreach (var worldObject in objectsCopy)
            {
                if (worldObject != null)
                {
                    worldObject.OnDespawn();
                }
            }

            _worldObjects.Clear();
            
            gameObject.SetActive(false);
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

