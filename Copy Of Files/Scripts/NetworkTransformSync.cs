using UnityEngine;
using Ubiq.Messaging;
using Ubiq.Avatars;  // Ubiq Avatar type is in this namespace
using UnityEngine;   // UnityEngine.Avatar also exists, so we create an alias for Ubiq's Avatar below

// Create an alias to refer specifically to Ubiq's Avatar.
using UbiqAvatar = Ubiq.Avatars.Avatar;

public class NetworkTransformSync : MonoBehaviour // No interface implementation needed
{
    private NetworkContext context;
    private Vector3 lastPosition;
    private Quaternion lastRotation;

    void Start()
    {
        // Register this component with Ubiq's networking system.
        context = NetworkScene.Register(this);
        lastPosition = transform.position;
        lastRotation = transform.rotation;
    }

    void Update()
    {
        // Only send updates if this is the local avatar.
        UbiqAvatar avatar = GetComponent<UbiqAvatar>();
        if (avatar != null && avatar.IsLocal)
        {
            if (Vector3.Distance(transform.position, lastPosition) > 0.01f ||
                Quaternion.Angle(transform.rotation, lastRotation) > 1f)
            {
                lastPosition = transform.position;
                lastRotation = transform.rotation;
                context.SendJson(new TransformMessage
                {
                    position = transform.position,
                    rotation = transform.rotation
                });
            }
        }
    }

    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Only update remote avatars.
        UbiqAvatar avatar = GetComponent<UbiqAvatar>();
        if (avatar != null && !avatar.IsLocal)
        {
            TransformMessage msg = message.FromJson<TransformMessage>();
            transform.position = msg.position;
            transform.rotation = msg.rotation;
        }
    }

    [System.Serializable]
    public class TransformMessage
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}