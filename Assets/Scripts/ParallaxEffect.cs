using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class ParallaxEffect : MonoBehaviour
{
    public Camera cam;
    public Transform followTarget;
    Vector2 startingPosition;
    float startingZ;

    Vector2 camMoveSinceStart => (Vector2) cam.transform.position - startingPosition;
    float parallaxFactor => Mathf.Abs(zDistanceFromTarget / clippingPlane);
    float zDistanceFromTarget => transform.position.z - followTarget.transform.position.z;
    float clippingPlane => cam.transform.position.z + (zDistanceFromTarget > 0 ? cam.farClipPlane : cam.nearClipPlane);

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startingPosition = transform.position;
        startingZ = transform.position.z;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector2 newPosition = startingPosition + camMoveSinceStart * parallaxFactor;
        transform.position = new Vector3(newPosition.x, newPosition.y, startingZ);
    }
}
