using UnityEngine;

public class AlignVRCameraToHead : MonoBehaviour
{
    public Transform vrCamera;  // The XR Camera (inside XR Origin)
    public Transform characterHead;  // The character's head bone

    void Update()
    {
        if (vrCamera != null && characterHead != null)
        {
            // Add an offset (e.g., 0.2f) so the camera is slightly above the head bone.
            vrCamera.position = characterHead.position + new Vector3(0, 1.0f, 0);
        }
    }
}
