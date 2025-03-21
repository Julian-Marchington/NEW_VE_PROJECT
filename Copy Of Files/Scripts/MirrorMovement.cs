using UnityEngine;

public class MirrorReflection : MonoBehaviour
{
    public Transform playerCamera;  // The player's VR headset (Main Camera in XR Origin)
    public Transform mirror;        // The mirror surface (Quad or Plane)
    private Camera mirrorCam;        // The Mirror Camera

    void Start()
    {
        mirrorCam = GetComponent<Camera>();
        if (mirrorCam == null)
        {
            Debug.LogError("Mirror Camera not found on this GameObject!");
        }
    }

    void LateUpdate()
    {
        if (playerCamera == null || mirror == null || mirrorCam == null) return;

        // Mirror the camera's position across the mirror plane
        Vector3 mirrorNormal = mirror.forward;  // Mirror's forward direction (normal)
        Vector3 toMirror = playerCamera.position - mirror.position; // Vector from mirror to player
        Vector3 reflectedPosition = playerCamera.position - 2 * Vector3.Dot(toMirror, mirrorNormal) * mirrorNormal;

        // Set the Mirror Camera's position
        mirrorCam.transform.position = reflectedPosition;

        // Mirror the camera's rotation
        Vector3 reflectedForward = Vector3.Reflect(playerCamera.forward, mirrorNormal);
        Vector3 reflectedUp = Vector3.Reflect(playerCamera.up, mirrorNormal);
        mirrorCam.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

        // Ensure the MirrorCam's FOV matches the player's
        mirrorCam.fieldOfView = Camera.main.fieldOfView;
    }
}