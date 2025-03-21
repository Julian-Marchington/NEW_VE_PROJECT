using Ubiq.Messaging;
using UnityEngine;

public class AvatarNetworkSync : MonoBehaviour
{
    NetworkContext context;

    void Start()
    {
        // Register this component with Ubiq's networking system.
        context = NetworkScene.Register(this);
    }

    /// <summary>
    /// Call this method when a local avatar part is changed.
    /// </summary>
    public void SendAvatarChange(string category, string variant)
    {
        // Create and send a message containing the change.
        AvatarChangeMessage msg = new AvatarChangeMessage
        {
            category = category,
            variant = variant
        };

        context.SendJson(msg);
        Debug.Log($"Sent avatar change: {category} -> {variant}");
    }

    /// <summary>
    /// Process incoming network messages.
    /// </summary>
    public void ProcessMessage(ReferenceCountedSceneGraphMessage message)
    {
        // Deserialize the JSON message.
        AvatarChangeMessage msg = message.FromJson<AvatarChangeMessage>();

        Debug.Log($"Received avatar change: {msg.category} -> {msg.variant}");

        // Apply the change to the avatar.
        // Note: You may need to verify that this update is for the intended avatar.
        if (AvatarSys._instance != null)
        {
            AvatarSys._instance.OnChangePart(msg.category, msg.variant);
        }
    }
}

/// <summary>
/// Message structure for avatar changes.
/// </summary>
[System.Serializable]
public struct AvatarChangeMessage
{
    public string category;
    public string variant;
}