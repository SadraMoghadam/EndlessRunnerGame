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

        private float activationDistance = 60f;
        private WorldChunk _parentChunk;
        private bool isActive = true;
        private Collider _collider;

        private float _moveSpeed = 0f;
        private ObjectData _configuredObjectData;
        private bool _isMoving;
        private bool _isDynamic;

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
            }
            if (_isMoving)
            {
                MoveWithWorld();
            }
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

