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

        private readonly Queue<WorldObstacle> _pool = new Queue<WorldObstacle>();
        private readonly HashSet<WorldObstacle> _active = new HashSet<WorldObstacle>();
        private Transform _root;

        public Transform Root => _root;

        private const string PoolFolderName = "DynamicObstacles";
        private const string WorldRootName = "World";

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            // Determine a scene-level World parent (avoid parenting under DontDestroyOnLoad objects)
            var targetParent = FindSceneWorldParent();

            if (targetParent != null)
            {
                // Reuse or create the single folder under World to hold dynamic obstacles (avoid duplicate nested folders)
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
                // No world parent yet: create a temporary root for pooled objects. This will be reparented in Start when World becomes available.
                var tempRoot = new GameObject(PoolFolderName);
                tempRoot.transform.SetParent(transform, false);
                _root = tempRoot.transform;
            }

            for (int i = 0; i < initialSize; i++)
            {
                var inst = CreateNewInstance();
                _pool.Enqueue(inst);
            }
        }

        private void Start()
        {
            // If World became available after Awake, reparent the root under it so pooled objects live under World
            EnsureRootParented();
        }

        public void EnsureRootParented()
        {
            if (_root == null) return;
            var wmParent = FindSceneWorldParent();
            if (wmParent != null)
            {
                var folder = wmParent.Find(PoolFolderName);
                if (folder == null)
                {
                    var folderGo = new GameObject(PoolFolderName);
                    folderGo.transform.SetParent(wmParent, false);
                    folder = folderGo.transform;
                }

                // If our current root isn't the canonical folder, move children and destroy temporary root
                if (_root != folder)
                {
                    // Move any pooled children under the canonical folder
                    var children = new List<Transform>();
                    for (int i = 0; i < _root.childCount; i++)
                    {
                        children.Add(_root.GetChild(i));
                    }

                    foreach (var child in children)
                    {
                        child.SetParent(folder, false);
                    }

                    // If the temporary root was a GameObject we created under this pool, destroy it
                    if (_root.gameObject.scene == gameObject.scene && _root.parent == transform)
                    {
                        Destroy(_root.gameObject);
                    }

                    _root = folder;
                }
            }
        }

        // Find a non-persistent scene-level parent for world objects.
        // Preference order:
        // 1) a GameObject named "World" in the active scene
        // 2) a WorldManager instance that is not in the DontDestroyOnLoad scene
        // 3) create a new GameObject named "World" in the active scene
        private Transform FindSceneWorldParent()
        {
            // 1) Try to find a GameObject named "World" in the active scene
            var worldGo = GameObject.Find(WorldRootName);
            if (worldGo != null && !IsPersistent(worldGo))
                return worldGo.transform;

            // 2) Search for WorldManager instances in scenes and pick one that's not persistent
            var managers = FindObjectsOfType<Managers.WorldManager>();
            foreach (var wm in managers)
            {
                if (wm != null && !IsPersistent(wm.gameObject))
                {
                    return wm.transform;
                }
            }

            // 3) If nothing found, create a scene-level World root in the active scene
            var activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                var created = new GameObject(WorldRootName);
                SceneManager.MoveGameObjectToScene(created, activeScene);
                return created.transform;
            }

            // As a last resort, return null so caller can temporarily parent to this pool
            return null;
        }

        private static bool IsPersistent(GameObject go)
        {
            return go != null && go.scene.name == "DontDestroyOnLoad";
        }

        private WorldObstacle CreateNewInstance()
        {
            var go = Instantiate(prefab, _root);
            var comp = go.GetComponent<WorldObstacle>();
            if (comp == null)
            {
                Debug.LogError("DynamicObstaclePool: prefab does not contain WorldObstacle component");
            }
            go.SetActive(false);
            return comp;
        }

        public WorldObstacle Get()
        {
            EnsureRootParented();
            WorldObstacle item = null;
            if (_pool.Count > 0)
            {
                item = _pool.Dequeue();
                item?.gameObject.SetActive(true);
            }
            else if (prefab != null && _pool.Count + 1 <= maxSize)
            {
                item = CreateNewInstance();
                item.gameObject.SetActive(true);
            }

            if (item != null)
            {
                _active.Add(item);
            }

            return item;
        }

        public void Return(WorldObstacle item)
        {
            if (item == null) return;

            if (_active.Contains(item)) _active.Remove(item);

            // put back under pool root and disable
            item.SetDormant(true);
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
    }
}
