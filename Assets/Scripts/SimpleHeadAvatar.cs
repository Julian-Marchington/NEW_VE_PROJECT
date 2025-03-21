using Ubiq.Avatars;
using UnityEngine;

namespace Ubiq.Samples
{
    /// <summary>
    /// A simplified avatar script that only updates the head transform based on network input.
    /// Hands, torso, and foot curves are ignored.
    /// </summary>
    public class SimpleHeadAvatar : MonoBehaviour
    {

        public Transform head;
        public Renderer headRenderer;

        private HeadAvatar headAndHandsAvatar;
        private InputVar<Pose> lastGoodHeadPose;

        private void OnEnable()
        {
            // Find the HeadAndHandsAvatar component in the parent.
            headAndHandsAvatar = GetComponentInParent<HeadAvatar>();
            if (headAndHandsAvatar != null)
            {
                headAndHandsAvatar.OnHeadUpdate.AddListener(HandleHeadUpdate);
            }
            else
            {
                Debug.LogWarning("SimpleHeadAvatar: No HeadAndHandsAvatar component found in parent.");
            }
        }

        private void OnDisable()
        {
            if (headAndHandsAvatar != null)
            {
                headAndHandsAvatar.OnHeadUpdate.RemoveListener(HandleHeadUpdate);
            }
        }

        private void HandleHeadUpdate(InputVar<Pose> pose)
        {
            // If the received head pose is invalid, try to use the last valid pose.
            if (!pose.valid)
            {
                if (!lastGoodHeadPose.valid)
                {
                    if (headRenderer != null)
                    {
                        headRenderer.enabled = false;
                    }
                    return;
                }
                pose = lastGoodHeadPose;
            }

            // Apply the valid head pose to the head transform.
            if (head != null)
            {
                head.position = pose.value.position;
                head.rotation = pose.value.rotation;
            }

            // Cache the last good head pose.
            lastGoodHeadPose = pose;

            // Re-enable the renderer if necessary.
            if (headRenderer != null && !headRenderer.enabled)
            {
                headRenderer.enabled = true;
            }
        }
    }
}