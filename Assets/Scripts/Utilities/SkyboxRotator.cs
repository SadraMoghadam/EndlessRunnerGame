using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private float rotationPerSecond = 1f;

    private float currentRotation;

    void Start()
    {
        if (skyboxMaterial == null)
        {
            Debug.LogError("Skybox material not assigned!");
            return;
        }
        RenderSettings.skybox = skyboxMaterial;
    }

    void Update()
    {
        if (RenderSettings.skybox == null) return;

        currentRotation += rotationPerSecond * Time.deltaTime;
        if (currentRotation > 360f) currentRotation -= 360f;

        skyboxMaterial.SetFloat("_Rotation", currentRotation);
    }

}
