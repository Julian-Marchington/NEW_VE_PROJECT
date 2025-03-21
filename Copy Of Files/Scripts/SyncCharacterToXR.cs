using UnityEngine;

public class SyncCharacterToXR : MonoBehaviour
{
    public Transform xrOrigin;  // XR Rig (VR Player)
    public Transform character; // Character model
    public LayerMask groundLayer; // Layer for the ground
    public float smoothSpeed = 10f; // Adjust for smoother movement

    public float groundOffset = 1.0f; // Set in the Inspector as needed
    private float fixedY = 0.6f;
    public float updateYInterval = 2.0f; // How often (in seconds) to update the ground height
    private float yUpdateTimer = 0f;

    void Start()
    {
        // Optionally, compute the initial floor height
        UpdateFixedY();
    }

    void UpdateFixedY()
    {
        if (Physics.Raycast(xrOrigin.position + Vector3.up * 0.5f, Vector3.down, out RaycastHit hit, 2f, groundLayer))
        {
            fixedY = hit.point.y + groundOffset;
        }
    }

    void Update()
    {
        // Update fixedY every updateYInterval seconds if needed
        yUpdateTimer += Time.deltaTime;
        if (yUpdateTimer >= updateYInterval)
        {
            UpdateFixedY();
            yUpdateTimer = 0f;
        }

        // Sync only the horizontal (X, Z) components from xrOrigin,
        // but keep the Y position fixed.
        Vector3 targetPosition = new Vector3(xrOrigin.position.x, fixedY, xrOrigin.position.z);
        character.position = Vector3.Lerp(character.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}