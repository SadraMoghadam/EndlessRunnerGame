using UnityEngine;
using UI;
using UnityEngine.SceneManagement;

namespace World
{
    [RequireComponent(typeof(Collider))]
    public class WorldObstacle : MonoBehaviour, IWorldObject
    {
        [Header("Obstacle Settings")]
        [SerializeField] private bool destroyOnDespawn = true;
        [SerializeField] private float damage = 1;

        // dynamic/moving-specific
        [Header("Dynamic Settings")]
        [Tooltip("If true, this obstacle behaves as a dynamic/moving obstacle that activates when the player is near.")]
        [SerializeField] private bool isMoving = false;
        [Tooltip("Distance (meters) from player at which this moving obstacle will start moving (and detach from its chunk).")]
        [SerializeField] private float activationDistance = 20f;
        [SerializeField] private float despawnBehindPlayerOffset = 5f;

        [SerializeField] private float minMoveSpeed = 7f;
        [SerializeField] private float maxMoveSpeed = 9f;


        private WorldChunk _parentChunk;
        private bool isActive = true;

        // moving state
        private bool _movementActive = false;
        private bool _isDormant = false;
        private Collider _collider;

        private float _moveSpeed = 0f;
        private ObjectData _configuredObjectData;

        public bool IsMoving => isMoving;

        /// <summary>
        /// Configures this obstacle from an ObjectData.
        /// </summary>
        public void ConfigureFromObjectData(ObjectData data)
        {
            if (data == null) return;

            _configuredObjectData = data;

            damage = data.damage;
            isMoving = data.isMoving;
            activationDistance = data.activationDistance;
            
            // Always use speed from ObjectData - set it directly
            _moveSpeed = data.speed;
            minMoveSpeed = data.speed;
            maxMoveSpeed = data.speed;
        }

        private void Awake()
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
        }

        private void Start()
        {
            // ensure collider disabled if dormant
            if (_isDormant && _collider != null)
            {
                _collider.enabled = false;
            }

            // If this is a non-moving obstacle, it should always be movement-active to respond to chunk movement
            if (!isMoving)
            {
                _movementActive = true;
                _isDormant = false;
            }
            else
            {
                // if moving and not dormant, ensure speed is set from configured ObjectData
                if (!_isDormant && _configuredObjectData != null)
                {
                    _moveSpeed = _configuredObjectData.speed;
                }
                else if (!_isDormant && _moveSpeed <= 0f)
                {
                    // Fallback only if not configured from ObjectData
                    _moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
                }
            }
        }

        private void Update()
        {
            if (!isActive) return;

            // For moving obstacles, check activation distance to player
            if (isMoving && !_movementActive)
            {
                var player = GameController.Instance != null ? GameController.Instance.PlayerController : null;
                if (player != null)
                {
                    float distanceAhead = transform.position.z - player.transform.position.z;
                    if (distanceAhead <= activationDistance)
                    {
                        ActivateMovement();
                    }
                }
            }

            // For moving obstacles that are active, they move opposite to world movement using their own speed
            if (isMoving && _movementActive)
            {
                // ensure we have a valid move speed - use configured ObjectData speed if available
                if (_moveSpeed <= 0f)
                {
                    if (_configuredObjectData != null)
                    {
                        _moveSpeed = _configuredObjectData.speed;
                    }
                    else
                    {
                        // Fallback only if not configured from ObjectData
                        _moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
                    }
                }

                float frameMove = _moveSpeed * Time.deltaTime;
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - frameMove);

                var player = GameController.Instance?.PlayerController;
                if (player != null && transform.position.z < player.transform.position.z - despawnBehindPlayerOffset)
                {
                    OnDespawn();
                }
            }
        }

        private void ActivateMovement()
        {
            // enable collider
            if (_collider != null) _collider.enabled = true;

            // Detach from chunk so chunk movement no longer affects this object
            // Parent under a scene-level World/DynamicObstacles folder so moving obstacles live inside the world hierarchy
            const string worldName = "World";
            const string folderName = "DynamicObstacles";

            GameObject worldRoot = GameObject.Find(worldName);
            if (worldRoot == null || (worldRoot.scene != null && worldRoot.scene.name == "DontDestroyOnLoad"))
            {
                var activeScene = SceneManager.GetActiveScene();
                if (activeScene.IsValid())
                {
                    worldRoot = new GameObject(worldName);
                    SceneManager.MoveGameObjectToScene(worldRoot, activeScene);
                }
            }

            Transform targetFolder = null;
            if (worldRoot != null)
            {
                targetFolder = worldRoot.transform.Find(folderName);
                if (targetFolder == null)
                {
                    var folderGo = new GameObject(folderName);
                    folderGo.transform.SetParent(worldRoot.transform, false);
                    SceneManager.MoveGameObjectToScene(folderGo, worldRoot.scene);
                    targetFolder = folderGo.transform;
                }
            }

            if (targetFolder != null)
            {
                transform.SetParent(targetFolder, false);
            }
            else
            {
                // fallback to no parent
                transform.SetParent(null);
            }

            // Remove from parent chunk registration since now independent
            if (_parentChunk != null)
            {
                _parentChunk.RemoveWorldObject(this);
                _parentChunk = null;
            }

            _isDormant = false;
            _movementActive = true;

            // Use speed from configured ObjectData if available, otherwise use random range
            if (_configuredObjectData != null)
            {
                _moveSpeed = _configuredObjectData.speed;
            }
            else
            {
                // Fallback only if not configured from ObjectData
                _moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
            }
        }

        public void SetDormant(bool dormant)
        {
            _isDormant = dormant;
            _movementActive = !dormant;
            if (_collider == null) _collider = GetComponent<Collider>();
            if (_collider != null) _collider.enabled = !dormant;

            // if set dormant, clear move speed so it will be set from ObjectData on activation
            if (dormant)
            {
                _moveSpeed = 0f;
            }
            else if (isMoving && _moveSpeed <= 0f)
            {
                // Use speed from configured ObjectData if available
                if (_configuredObjectData != null)
                {
                    _moveSpeed = _configuredObjectData.speed;
                }
                else
                {
                    // Fallback only if not configured from ObjectData
                    _moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);
                }
            }
        }

        /// <summary>
        /// Resets the obstacle to its initial state so it can be reused from the pool.
        /// </summary>
        public void Reset()
        {
            isActive = true;
            _movementActive = false;
            _isDormant = true;
            _moveSpeed = 0f;
            _configuredObjectData = null;
            
            // Clear parent chunk reference (should already be null for moving obstacles, but ensure it)
            if (_parentChunk != null)
            {
                _parentChunk.RemoveWorldObject(this);
                _parentChunk = null;
            }

            // Ensure collider is properly set up
            if (_collider == null) _collider = GetComponent<Collider>();
            if (_collider != null)
            {
                // Ensure it's a trigger (as required by the component)
                if (!_collider.isTrigger)
                {
                    _collider.isTrigger = true;
                }
                _collider.enabled = false;
            }
        }

        public void MoveWithWorld()
        {
            if (!isActive) return;

            if (isMoving && _movementActive)
            {
                // if system calls this directly, interpret deltaMovement as an external movement and apply towards player
                transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z + _moveSpeed);
            }
            else
            {
                // static obstacles / chunk-managed objects move with chunk (positive delta)
                transform.position = new Vector3(
                    transform.position.x,
                    transform.position.y,
                    transform.position.z + _moveSpeed
                );
            }
        }

        public void OnCollided()
        {
            // deal damage
            if (GameController.Instance != null)
            {
                GameController.Instance.GameOver();
            }
        }

        public void OnDespawn()
        {
            isActive = false;

            // If this is a moving obstacle and a pool exists, return to pool instead of destroying
            if (isMoving)
            {
                var pool = DynamicObstaclePool.Instance;
                if (pool != null)
                {
                    // always return moving obstacles to the pool when available
                    pool.Return(this);
                    return;
                }

                // if no pool, just deactivate the moving obstacle instead of destroying
                if (_parentChunk != null)
                {
                    _parentChunk.RemoveWorldObject(this);
                }

                gameObject.SetActive(false);
                return;
            }

            if (_parentChunk != null)
            {
                _parentChunk.RemoveWorldObject(this);
            }

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

