using UnityEngine;
using UI;

namespace World
{
    [RequireComponent(typeof(Collider))]
    public class WorldObstacle : MonoBehaviour, IWorldObject
    {
        [Header("Obstacle Settings")]
        [SerializeField] private bool destroyOnDespawn = true;
        [SerializeField] private int damage = 1;

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
            if (parentChunk != null)
            {
                parentChunk.RemoveWorldObject(this);
            }
        }
    }
}

