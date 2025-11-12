using UnityEngine;
using System.Collections.Generic;

namespace World
{
    public class WorldChunk : MonoBehaviour
    {
        [Header("Chunk Settings")]
        [SerializeField] private float chunkLength = 20f;
        [SerializeField] private Transform chunkStartPoint;
        [SerializeField] private Transform chunkEndPoint;
        
        
        private readonly Lane _leftLane = new Lane(LaneNumber.Left, -7.5f, -2.5f);
        private readonly Lane _centerLane = new Lane(LaneNumber.Left, -2.5f, 2.5f);
        private readonly Lane _rightLane = new Lane(LaneNumber.Left, 2.5f, 7.5f);
        
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
            
            for (int i = _worldObjects.Count - 1; i >= 0; i--)
            {
                if (_worldObjects[i] != null)
                {
                    _worldObjects[i].MoveWithWorld(deltaMovement);
                }
                else
                {
                    _worldObjects.RemoveAt(i);
                }
            }
        }
        
        public float GetLaneXPosition(LaneNumber lane)
        {
            return lane switch
            {
                LaneNumber.Left => _leftLane.Center,
                LaneNumber.Center => _centerLane.Center,
                LaneNumber.Right => _rightLane.Center,
                _ => _centerLane.Center
            };
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
            
            foreach (var worldObject in _worldObjects)
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
                
                Gizmos.DrawLine(start + new Vector3(_leftLane.Center, 0, 0), end + new Vector3(_leftLane.Center, 0, 0));
                Gizmos.DrawLine(start + new Vector3(_centerLane.Center, 0, 0), end + new Vector3(_centerLane.Center, 0, 0));
                Gizmos.DrawLine(start + new Vector3(_rightLane.Center, 0, 0), end + new Vector3(_rightLane.Center, 0, 0));
            }
        }
    }
}

