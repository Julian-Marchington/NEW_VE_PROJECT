using System.Collections;
using UnityEngine;

public class DefaultPartsApplier : MonoBehaviour
{
    // Define defaults here, must match the naming used in AvatarCustomizer.
    private string[,] defaultParts = new string[,] {
        {"Hair", "1"},
        {"Top", "1"},
        {"Bottom", "1"},
        {"Shoes", "1"},
        {"Face", "A1"}
    };

    void Start()
    {
        StartCoroutine(ApplyDefaults());
    }

    IEnumerator ApplyDefaults()
    {
        // Wait a short delay to ensure AvatarCustomizer has run.
        yield return new WaitForSeconds(0.5f);

        AvatarCustomizer customizer = GetComponent<AvatarCustomizer>();
        if (customizer == null)
        {
            Debug.LogError("DefaultPartsApplier: No AvatarCustomizer found on this avatar!");
            yield break;
        }

        int rows = defaultParts.GetLength(0);
        for (int i = 0; i < rows; i++)
        {
            string category = defaultParts[i, 0];
            string variant = defaultParts[i, 1];
            customizer.OnChangePart(category, variant);
            Debug.Log($"DefaultPartsApplier: Applied default for {category} -> {variant}");
            yield return null; // Optionally wait a frame between changes.
        }
    }
}