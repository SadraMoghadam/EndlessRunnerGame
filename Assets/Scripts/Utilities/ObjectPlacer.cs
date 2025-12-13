using UnityEditor.Rendering;
using UnityEngine;
using World;

public class ObjectPlacer : MonoBehaviour
{

    [SerializeField] private ObjectsContainerSO objectsContainer;
    [SerializeField] private float speed;
    private ObjectData _currentObjectData;
    private GameObject _currentObject;


    private void Awake()
    {
        if (_currentObjectData != null)
            return;
        GetComponent<Renderer>().enabled = false;
        if (speed == 0)
        {
            _currentObjectData = objectsContainer.GetRandomObject();
            _currentObject = Instantiate(_currentObjectData.objectPrefab, transform.position, Quaternion.identity, transform);
        }
        else
        {
            _currentObjectData = objectsContainer.GetRandomObjectBySpeed(speed);
            if (_currentObjectData.objectPrefab != null &&
                _currentObjectData.objectPrefab.GetComponent<WorldObstacle>() != null)
            {
                _currentObject = Instantiate(_currentObjectData.objectPrefab, transform.position, Quaternion.identity, transform);
                _currentObject.GetComponent<WorldObstacle>().ConfigureFromObjectData(_currentObjectData);
            }

        }
    }
}
