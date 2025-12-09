using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Managers;

namespace World
{
    public class DynamicObstaclePool : MonoBehaviour
    {
        public static DynamicObstaclePool Instance { get; private set; }

        [Tooltip("Prefab for pooled moving obstacle. Should have WorldObstacle component configured as moving.")]
        [SerializeField] private GameObject prefab;
        [SerializeField] private int initialSize = 10;
        [SerializeField] private int maxSize = 50;
        [SerializeField] private WorldMover World;
        [SerializeField] private ObjectConfigSO dynamicObstacleConfig;

        private readonly Queue<WorldObstacle> _pool = new Queue<WorldObstacle>();
        private readonly HashSet<WorldObstacle> _active = new HashSet<WorldObstacle>();
        private Transform _root;

        public Transform Root => _root;

        private const string PoolFolderName = "DynamicObstacles";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            var targetParent = World.transform;

            if (targetParent != null)
            {
                var folder = targetParent.Find(PoolFolderName);
                if (folder == null)
                {
                    var folderGo = new GameObject(PoolFolderName);
                    folderGo.transform.SetParent(targetParent, false);
                    _root = folderGo.transform;
                }
                else
                {
                    _root = folder;
                }
            }
            else
            {
                return;
            }

            for (int i = 0; i < initialSize; i++)
            {
                var inst = CreateNewInstance();
                _pool.Enqueue(inst);
            }
        }

        private WorldObstacle CreateNewInstance()
        {
            ObjectData objectData = dynamicObstacleConfig?.GetRandomObject();
            var go = Instantiate(objectData.objectPrefab, _root);
            var comp = go.GetComponent<WorldObstacle>();
            if (comp == null)
            {
                Debug.LogError("DynamicObstaclePool: prefab does not contain WorldObstacle component");
            }
            comp.ConfigureFromObjectData(objectData);
            go.SetActive(false);
            return comp;
        }

        public WorldObstacle Get()
        {
            WorldObstacle item = null;
            if (_pool.Count > 0)
            {
                item = _pool.Dequeue();
                if (item != null)
                {
                    // Ensure the obstacle is reset before reuse (in case it wasn't properly reset)
                    item.Reset();
                    item.gameObject.SetActive(true);
                }
            }
            else if (prefab != null && _pool.Count + 1 <= maxSize)
            {
                item = CreateNewInstance();
                if (item != null)
                {
                    item.gameObject.SetActive(true);
                }
            }

            if (item != null)
            {
                _active.Add(item);
            }

            if (item != null)
            {
                if (GameManager.Instance.NoCollisionMode)
                    item.GetComponent<Collider>().enabled = false;
                else
                    item.GetComponent<Collider>().enabled = true;
            }

            return item;
        }

        public void Return(WorldObstacle item)
        {
            if (item == null) return;

            if (_active.Contains(item)) _active.Remove(item);

            // Reset the obstacle to its initial state so it can be reused
            item.Reset();
            
            // put back under pool root and disable
            item.transform.SetParent(_root, false);
            item.gameObject.SetActive(false);

            if (!_pool.Contains(item))
            {
                _pool.Enqueue(item);
            }
        }

        public void ReturnAll()
        {
            // Copy to avoid modification during iteration
            var copy = new List<WorldObstacle>(_active);
            foreach (var item in copy)
            {
                Return(item);
            }
        }

        public void DeactivatePassedMovingObstacles(float playerZ, float despawnBehindOffset)
        {
            if (_active.Count == 0) return;

            var copy = new List<WorldObstacle>(_active);
            foreach (var item in copy)
            {
                if (item == null) continue;

                // only consider moving obstacles
                if (!item.IsMoving) continue;

                // if behind player beyond threshold, return to pool
                if (item.transform.position.z < playerZ - despawnBehindOffset)
                {
                    Return(item);
                }
            }
        }
    }
}
