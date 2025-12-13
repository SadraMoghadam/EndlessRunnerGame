using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PrefabSO", menuName = "Scriptable Objects/PrefabSO")]
public class PrefabContainerSO : ScriptableObject
{
    public List<GameObject> prefabs;

    public GameObject getRandom()
    {
        if (prefabs == null)
        {
            Debug.LogError("PrefabContainerSO: prefabs list is null.");
            return null;
        }
        return prefabs[Random.Range(0, prefabs.Count)];
    }
}
