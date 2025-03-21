using UnityEngine;
using Ubiq.Avatars; // Alias conflicts resolved elsewhere if needed.

public class LocalAvatarFollow : MonoBehaviour
{
    [Tooltip("The transform of your XR rig (will be auto-assigned if left null).")]
    public Transform xrOrigin;

    void Awake()
    {
        // If not assigned in the Inspector, find the XR Origin by tag.
        if (xrOrigin == null)
        {
            GameObject found = GameObject.FindWithTag("XROrigin");
            if (found != null)
            {
                xrOrigin = found.transform;
                Debug.Log("XR Origin auto-assigned from tag.");
            }
            else
            {
                Debug.LogError("No XR Origin found! Please tag your XR rig with 'XROrigin'.");
            }
        }
    }

    void Update()
    {
        // Only update if this is the local avatar.
        Ubiq.Avatars.Avatar avatar = GetComponent<Ubiq.Avatars.Avatar>();
        if (avatar != null && avatar.IsLocal && xrOrigin != null)
        {
            // Set the world position/rotation to match the XR rig.
            transform.position = xrOrigin.position;
            transform.rotation = xrOrigin.rotation;
        }
    }
}