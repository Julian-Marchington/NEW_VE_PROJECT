using UnityEngine;
using Ubiq.Avatars;

#if XRI_3_0_7_OR_NEWER
using Unity.XR.CoreUtils;
using UnityEngine.XR.Interaction.Toolkit;
#endif

namespace Ubiq.XRI
{
    /// <summary>
    /// A modified input script that only provides head input.
    /// The hand and grip values are set to invalid.
    /// </summary>
    public class HeadAvatarInputXR : MonoBehaviour
    {
        [Tooltip("The AvatarManager to provide input to. If null, will try to find one in the scene at start.")]
        [SerializeField] private AvatarManager avatarManager;
        [Tooltip("The GameObject containing the XROrigin. If null, will try to find one in the scene at Start.")]
        [SerializeField] private GameObject xrOriginGameObject;
        [Tooltip("Higher priority inputs will override lower priority inputs of the same type if multiple exist.")]
        public int priority = 0;

#if XRI_3_0_7_OR_NEWER
        private class HeadInput : IHeadAndHandsInput
        {
            public int priority => owner.priority;
            public bool active => owner.isActiveAndEnabled;

            // Only provide head input. The hand and grip inputs are not used.
            public InputVar<Pose> head => owner.Head();
            public InputVar<Pose> leftHand => InputVar<Pose>.invalid;
            public InputVar<Pose> rightHand => InputVar<Pose>.invalid;
            public InputVar<float> leftGrip => InputVar<float>.invalid;
            public InputVar<float> rightGrip => InputVar<float>.invalid;

            private HeadAvatarInputXRI owner;
            public HeadInput(HeadAvatarInputXRI owner)
            {
                this.owner = owner;
            }
        }
        
        private XROrigin origin;
        private XRInputModalityManager modalityManager;
        private HeadInput input;
        
        private void Start()
        {
            if (!avatarManager)
            {
                avatarManager = FindAnyObjectByType<AvatarManager>();
                if (!avatarManager)
                {
                    Debug.LogWarning("No AvatarManager could be found in this Unity scene. Disabling script.");
                    enabled = false;
                    return;
                }
            }
            
            if (xrOriginGameObject)
            {
                origin = xrOriginGameObject.GetComponent<XROrigin>();
                if (!origin)
                {
                    Debug.LogWarning("XROriginGameObject supplied but no XROrigin component found. Attempting to find one in scene.");
                }
            }
            if (!origin)
            {
                origin = FindObjectOfType<XROrigin>();
                if (!origin)
                {
                    Debug.LogWarning("No XROrigin found. The local avatar will not have its input driven by XRI.");
                    return;
                }
            }
            
            modalityManager = origin.GetComponentInChildren<XRInputModalityManager>();
            if (!modalityManager)
            {
                Debug.LogWarning("No XRInputModalityManager found as a child of XROrigin. Cannot provide input. Disabling script.");
                enabled = false;
            }
            
            input = new HeadInput(this);
            avatarManager.input.Add(input);
        }
        
        private void OnDestroy()
        {
            if (avatarManager)
            {
                avatarManager.input?.Remove(input);
            }
        }
        
        private InputVar<Pose> Head()
        {
            var cam = origin.Camera;
            if (!cam)
            {
                return InputVar<Pose>.invalid;
            }
                    
            cam.transform.GetPositionAndRotation(out var p, out var r);
            return new InputVar<Pose>(new Pose(p, r));
        }
#endif
    }
}