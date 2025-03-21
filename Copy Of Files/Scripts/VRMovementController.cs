using UnityEngine;
using Ubiq.Avatars;

public class VRMovementController : MonoBehaviour
{
    public Transform xrRig; // The root of your VR rig
    private Vector3 lastPosition;

    void Start()
    {
        // Initialize lastPosition with the current XR rig position.
        lastPosition = xrRig.position;
    }

    void Update()
    {
        // Calculate how much the XR rig has moved since the last frame.
        float distanceMoved = Vector3.Distance(xrRig.position, lastPosition);
        float speed = distanceMoved / Time.deltaTime;

        // Find the AvatarManager in the scene
        AvatarManager avatarManager = FindObjectOfType<AvatarManager>();
        if (avatarManager != null && avatarManager.LocalAvatar != null)
        {
            // Get the Animator from the local avatar
            Animator animator = avatarManager.LocalAvatar.GetComponent<Animator>();
            if (animator != null)
            {
                // Update the 'Speed' parameter on the avatar's Animator
                animator.SetFloat("Speed", speed);
            }
            else
            {
                Debug.LogWarning("Local avatar has no Animator component!");
            }
        }
        else
        {
            Debug.LogWarning("Local avatar or AvatarManager not found or not yet spawned.");
        }

        // Store the current position for the next frame.
        lastPosition = xrRig.position;
    }
}