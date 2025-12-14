using UnityEngine;
using UI;
using UnityEngine.SceneManagement;


namespace World
{
    [RequireComponent(typeof(Collider))]
    public class WorldObstacle : MonoBehaviour, IWorldObject
    {
        [Header("Obstacle Settings")]
        [SerializeField] private bool destroyOnDespawn = false;
        [SerializeField] private float damage = 1;

        [Header("Collision Detection")]
        [SerializeField] private float lookAheadDistance = 2f;
        [SerializeField] private LayerMask obstacleLayerMask = -1; // Check all layers by default

        private float activationDistance = 60f;
        private WorldChunk _parentChunk;
        private bool isActive = true;
        private Collider _collider;

        private float _moveSpeed = 0f;
        private ObjectData _configuredObjectData;
        private bool _isMoving;
        private bool _isDynamic;
        private bool _isBlocked = false;

        // Formula = _moveSpeed * (activationDistance / (worldSpeed - _moveSpeed))

        public void ConfigureFromObjectData(ObjectData data)
        {
            if (data == null) return;

            _configuredObjectData = data;

            damage = data.damage;
            _moveSpeed = data.speed;
            _isDynamic = _moveSpeed > 0f;
        }

        private void Start()
        {
            _parentChunk = GetComponentInParent<WorldChunk>();
            if (_parentChunk != null)
            {
                _parentChunk.AddWorldObject(this);
            }

            _collider = GetComponent<Collider>();
            if (_collider != null && !_collider.isTrigger)
            {
                _collider.isTrigger = true;
            }
            ConfigureFromObjectData(_configuredObjectData);
        }

        private void Update()
        {
            if (!isActive) return;

            if (_isDynamic)
            {
                var player = GameController.Instance != null ? GameController.Instance.PlayerController : null;
                if (player != null)
                {
                    float distanceAhead = transform.position.z - player.transform.position.z;
                    if (distanceAhead <= activationDistance)
                    {
                        _isMoving = true;
                    }
                }

                // Check for obstacles in front before moving
                if (_isMoving && !_isBlocked)
                {
                    _isBlocked = CheckForObstacleInFront();
                    if (!_isBlocked)
                    {
                        MoveWithWorld();
                    }
                }
            }
        }

        private bool CheckForObstacleInFront()
        {
            Vector3 origin = transform.position + new Vector3(0, 0.5f, 0);
            Vector3 direction = Vector3.forward;
            
            // Use Raycast to detect obstacles in front (single line cast)
            RaycastHit hit;
            bool hasHit = Physics.Raycast(
                origin,
                direction,
                out hit,
                lookAheadDistance,
                obstacleLayerMask
            );

            if (hasHit)
            {
                if (hit.collider.gameObject == gameObject)
                {
                    return false;
                }

                // Check if it's another obstacle (static or dynamic)
                WorldObstacle otherObstacle = hit.collider.GetComponent<WorldObstacle>();
                if (otherObstacle != null)
                {
                    return true;
                }

                // Also check for any collider that's not a trigger (static obstacles)
                if (!hit.collider.isTrigger)
                {
                    return true;
                }
            }

            return false;
        }

        public void MoveWithWorld()
        {
            transform.Translate(Vector3.forward * _moveSpeed * Time.deltaTime);
        }

        public void OnCollided()
        {
            if (GameController.Instance != null)
            {
                GameController.Instance.GameOver();
            }
        }

        public void ResetMoving()
        {
            _isMoving = false;
            _isBlocked = false;
        }

        public void OnDespawn()
        {
            isActive = false;

            if (destroyOnDespawn)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!isActive) return;
            if (other.CompareTag("Player"))
            {
                OnCollided();
                if (destroyOnDespawn) Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (_parentChunk != null)
            {
                _parentChunk.RemoveWorldObject(this);
            }
        }
    }
}

