using UnityEngine;

namespace World
{
    [RequireComponent(typeof(Collider))]
    public class WorldObstacle : MonoBehaviour, IWorldObject
    {
        [Header("Obstacle Settings")]
        [SerializeField] private bool destroyOnDespawn = true;
        
        private WorldChunk parentChunk;
        private bool isActive = true;
        
        private void Awake()
        {
            parentChunk = GetComponentInParent<WorldChunk>();
            if (parentChunk != null)
            {
                parentChunk.AddWorldObject(this);
            }
        }
        
        public void MoveWithWorld(float deltaMovement)
        {
            if (!isActive) return;
            
            transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                transform.position.z - deltaMovement
            );
        }
        
        public void OnDespawn()
        {
            isActive = false;
            
            if (parentChunk != null)
            {
                parentChunk.RemoveWorldObject(this);
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
        
        private void OnDestroy()
        {
            if (parentChunk != null)
            {
                parentChunk.RemoveWorldObject(this);
            }
        }
    }
}

