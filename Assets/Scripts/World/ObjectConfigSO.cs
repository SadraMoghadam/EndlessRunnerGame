using UnityEngine;

namespace World
{
    [System.Serializable]
    public class ObjectData
    {
        [Header("Object Properties")]
        public string objectName = "New Object";
        public GameObject objectPrefab;
        public float speed = 0;
        public float damage = 0;
        public bool isMoving = false;
        public float activationDistance = 20f;
    }

    [CreateAssetMenu(fileName = "ObjectData", menuName = "World/Object Config", order = 2)]
    public class ObjectConfigSO : ScriptableObject
    {
        public ObjectData[] objectConfigs = new ObjectData[0];

        public ObjectData GetRandomObject()
        {
            if (objectConfigs == null || objectConfigs.Length == 0)
            {
                Debug.LogWarning($"ObjectConfigSO '{name}' has no object configurations.");
                return null;
            }

            int randomIndex = Random.Range(0, objectConfigs.Length);
            return objectConfigs[randomIndex];
        }
    }
}

