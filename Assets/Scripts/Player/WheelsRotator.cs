using UnityEngine;

public class WheelsRotator : MonoBehaviour
{
    [SerializeField] private Transform[] wheels;
    [SerializeField] private float rotationSpeed = 360f;

    void Update()
    {
        float rot = rotationSpeed * Time.deltaTime;
        foreach (var wheel in wheels)
            wheel.Rotate(Vector3.right, rot, Space.Self);
    }
}