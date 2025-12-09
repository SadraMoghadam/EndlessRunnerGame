using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider))]
    public class WorldCollectible : MonoBehaviour, IWorldObject
    {
        [Header("Collectible Settings")]
        [SerializeField] private int scoreValue = 10;
        [SerializeField] private bool destroyOnDespawn = true;
        [SerializeField] private float rotationSpeed = 90f;
        
        private WorldChunk _parentChunk;
        private bool _isActive = true;
        private bool _isCollected = false;
        
        private void Awake()
        {
            _parentChunk = GetComponentInParent<WorldChunk>();
            if (_parentChunk != null)
            {
                _parentChunk.AddWorldObject(this);
            }
        }
        
        private void Update()
        {
            if (_isActive && !_isCollected)
            {
                transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            }
        }
        
        public void MoveWithWorld()
        {
            return;
            //if (!_isActive) return;
            //transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - deltaMovement);
        }
        
        public void OnDespawn()
        {
            _isActive = false;
            
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
        
        public void OnCollided()
        {
            if (_isCollected) return;
            
            _isCollected = true;

            if (GameController.Instance != null)
            {
                GameController.Instance.SetCoins(GameController.Instance.GameCoins + scoreValue);
            }
            
            OnDespawn();
        }
        
        private void OnTriggerEnter(Collider other)
        {
            if (_isCollected) return;
            
            if (other.CompareTag("Player"))
            {
                OnCollided();
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

