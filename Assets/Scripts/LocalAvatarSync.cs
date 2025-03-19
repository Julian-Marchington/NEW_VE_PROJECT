using UnityEngine;

public class LocalAvatarSync : MonoBehaviour
{
    [Tooltip("Reference to your XR Origin (the root of your VR rig).")]
    public Transform xrOrigin;

    void Update()
    {
        // If xrOrigin is assigned, make the local avatar follow it.
        if (xrOrigin != null)
        {
            transform.position = xrOrigin.position;
            transform.rotation = xrOrigin.rotation;
        }
    }
}