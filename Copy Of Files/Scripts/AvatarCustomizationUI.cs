using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Ubiq.Avatars; // for Avatar, if needed

public class AvatarCustomizationUI : MonoBehaviour
{
    [Header("Category Settings")]
    [Tooltip("Enter the category name exactly as used in your AvatarCustomizer (e.g., \"Hair\", \"Top\", \"Bottom\", \"Shoes\", \"Face\").")]
    public string category;

    [Header("UI References")]
    [Tooltip("The parent container where buttons will be added (e.g., the Content object of a Scroll View).")]
    public Transform buttonContainer;
    [Tooltip("A prefab for a UI Button that contains a Text component to display the variant name.")]
    public Button buttonPrefab;

    IEnumerator Start()
    {
        // Wait one frame to allow the Avatar Manager to spawn the local avatar.
        yield return null;

        // Try to locate the Avatar Manager in the scene.
        AvatarManager avatarManager = FindObjectOfType<AvatarManager>();
        if (avatarManager == null)
        {
            Debug.LogWarning("AvatarManager not found in the scene.");
            yield break;
        }

        // Ensure the local avatar has been spawned.
        if (avatarManager.LocalAvatar == null)
        {
            Debug.LogWarning("Local avatar not yet spawned by AvatarManager.");
            yield break;
        }

        // Get the AvatarCustomizer from the local avatar.
        AvatarCustomizer customizer = avatarManager.LocalAvatar.GetComponent<AvatarCustomizer>();
        if (customizer == null)
        {
            Debug.LogWarning("AvatarCustomizer component not found on the local avatar.");
            yield break;
        }

        // Query available variants from the AvatarCustomizer.
        Dictionary<string, SkinnedMeshRenderer> parts;
        if (customizer.GetPartsForCategory(category, out parts))
        {
            Debug.Log("Found " + parts.Count + " parts for category: " + category);
            foreach (KeyValuePair<string, SkinnedMeshRenderer> kvp in parts)
            {
                // Instantiate a new button.
                Button newButton = Instantiate(buttonPrefab, buttonContainer);

                // Set the button's GameObject name to the key.
                newButton.name = kvp.Key;

                // Get the Text component on the button and update its text.
                Text btnText = newButton.GetComponentInChildren<Text>();
                if (btnText != null)
                {
                    btnText.text = kvp.Key;
                }
                else
                {
                    Debug.LogWarning("Button prefab is missing a Text component.");
                }

                // Assuming the key is in the format "Category_Variant" (e.g., "Hair_1"),
                // extract the variant portion.
                string variantNumber = kvp.Key.Substring(category.Length + 1);
                string part = category;
                string variant = variantNumber;

                // Capture the current values for the lambda.
                string capturedPart = part;
                string capturedVariant = variant;

                newButton.onClick.AddListener(() =>
                {
                    // 1. Update the local avatar's customization.
                    customizer.OnChangePart(capturedPart, capturedVariant);

                    // 2. If you want to broadcast changes, check for the AvatarNetworkSync component.
                    AvatarNetworkSync sync = avatarManager.LocalAvatar.GetComponent<AvatarNetworkSync>();
                    if (sync != null)
                    {
                        sync.SendAvatarChange(capturedPart, capturedVariant);
                    }
                    else
                    {
                        Debug.LogWarning("AvatarNetworkSync component is missing on the local avatar!");
                    }
                });
            }
        }
        else
        {
            Debug.LogWarning("No parts found for category: " + category);
        }
    }
}