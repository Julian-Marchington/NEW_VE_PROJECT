using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarSys : MonoBehaviour
{
    public static AvatarSys _instance;

    [Header("Prefabs")]
    [SerializeField] private GameObject modelPrefab;   // Resource model containing all available parts (with a "Parts" folder)
    [SerializeField] private GameObject targetPrefab;    // Your visible avatar prefab (e.g., Character_22)

    [Header("XR Setup")]
    public Transform xrOrigin; // Reference to your XR Origin (the root GameObject of your VR rig)

    // Private fields
    private Transform sourceTrans;
    private GameObject target;
    private Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data = new Dictionary<string, Dictionary<string, SkinnedMeshRenderer>>();
    private Transform[] hips;
    private Dictionary<string, SkinnedMeshRenderer> smr = new Dictionary<string, SkinnedMeshRenderer>();

    // Default parts selection: { category, variant }
    private string[,] avatarStr = new string[,] {
        { "Hair", "1" },
        { "Top", "1" },
        { "Bottom", "1" },
        { "Shoes", "1" },
        { "Face", "A1" }
    };

    // Public accessor for the instantiated avatar
    public GameObject CurrentAvatar => target;
    // New public property to store the Animator on the instantiated avatar
    public Animator CurrentAnimator { get; private set; }

    void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        InstantiateAvatar();
        SaveData(sourceTrans, data, target, smr);
        InitAvatar();
    }

    // Instantiate the source (hidden) and target (visible) avatars as children of the XR Origin.
    void InstantiateAvatar()
    {
        if (modelPrefab == null || targetPrefab == null || xrOrigin == null)
        {
            Debug.LogError("Please assign the modelPrefab, targetPrefab, and xrOrigin!");
            return;
        }

        // Instantiate the source model (hidden repository for parts) as a child of XR Origin.
        GameObject sourceGO = Instantiate(modelPrefab, xrOrigin.position, Quaternion.identity, xrOrigin);
        sourceTrans = sourceGO.transform;
        sourceGO.SetActive(false);

        // Instantiate the target (visible avatar) as a child of XR Origin.
        target = Instantiate(targetPrefab, xrOrigin.position, Quaternion.identity, xrOrigin);
        hips = target.GetComponentsInChildren<Transform>();

        // Reset local position/rotation
        target.transform.localPosition = Vector3.zero;
        target.transform.localRotation = Quaternion.identity;

        // Add an upward spawn offset (adjust this value as needed so that the avatar sits properly on the ground)
        float spawnYOffset = 0f;
        target.transform.localPosition += new Vector3(0, spawnYOffset, 0);

        // Capture the Animator component from the instantiated avatar.
        CurrentAnimator = target.GetComponent<Animator>();
        if (CurrentAnimator == null)
        {
            Debug.LogError("No Animator component found on the target avatar prefab!");
        }

        // Attach and configure the SyncCharacterToXR script.
        SyncCharacterToXR syncScript = target.AddComponent<SyncCharacterToXR>();
        syncScript.xrOrigin = xrOrigin;
        syncScript.character = target.transform;

        // Optionally attach the AlignVRCameraToHead script if needed.
        // AlignVRCameraToHead alignScript = target.AddComponent<AlignVRCameraToHead>();
        // alignScript.vrCamera = xrOrigin.Find("Camera Offset/Main Camera");
        // alignScript.characterHead = target.transform.Find("Bone/QuickRigCharacter2_Reference/QuickRigCharacter2_Hips/QuickRigCharacter2_Spine/QuickRigCharacter2_Spine1/QuickRigCharacter2_Spine2/QuickRigCharacter2_Neck/QuickRigCharacter2_Head");

        Debug.Log("Avatar instantiated with adjusted spawn offset.");
    }

    // Collects part data from sourceTrans/Parts and creates corresponding empty target parts.
    void SaveData(Transform sourceTrans, Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data, GameObject target, Dictionary<string, SkinnedMeshRenderer> smr)
    {
        data.Clear();
        smr.Clear();

        if (sourceTrans == null)
            return;

        // Look for the "Parts" folder in the source model.
        Transform partsParent = sourceTrans.Find("Parts");
        if (partsParent == null)
        {
            Debug.LogError("No 'Parts' folder found in source model!");
            return;
        }

        // Iterate through each category (e.g., Hair, Top, etc.)
        foreach (Transform category in partsParent)
        {
            if (!data.ContainsKey(category.name))
                data.Add(category.name, new Dictionary<string, SkinnedMeshRenderer>());

            // Find (or create) the corresponding category folder in the target.
            Transform targetPartsParent = target.transform.Find("Parts");
            if (targetPartsParent == null)
            {
                Debug.LogError("No 'Parts' folder found in target model!");
                continue;
            }
            Transform targetCategory = targetPartsParent.Find(category.name);
            if (targetCategory == null)
            {
                GameObject newCategory = new GameObject(category.name);
                newCategory.transform.parent = targetPartsParent;
                targetCategory = newCategory.transform;
            }

            // For each part in the category (e.g., Hair_1, Hair_2, etc.)
            foreach (Transform part in category)
            {
                SkinnedMeshRenderer partSMR = part.GetComponent<SkinnedMeshRenderer>();
                if (partSMR != null)
                {
                    // Key name should match: e.g., "Hair_1"
                    data[category.name].Add(part.name, partSMR);
                    Debug.Log("Collected part: " + part.name + " under category: " + category.name);
                }
            }

            // Create an empty SkinnedMeshRenderer on the target for this category if not already created.
            if (!smr.ContainsKey(category.name))
            {
                GameObject partGo = new GameObject(category.name);
                partGo.transform.parent = targetCategory;
                smr.Add(category.name, partGo.AddComponent<SkinnedMeshRenderer>());
            }
        }
    }

    // Changes the target's mesh for a given category using the provided variant number.
    // 'part' is the category (e.g., "Hair") and 'num' is the variant (e.g., "1" for "Hair_1").
    void ChangeMesh(string part, string num, Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data, Transform[] hips, Dictionary<string, SkinnedMeshRenderer> smr, string[,] str)
    {
        string fullPartName = part + "_" + num;

        if (!data.ContainsKey(part) || !data[part].ContainsKey(fullPartName))
        {
            Debug.LogError("No part found with key " + fullPartName);
            return;
        }

        SkinnedMeshRenderer skm = data[part][fullPartName];

        // Remap bones from the source part to the target's bones.
        List<Transform> bones = new List<Transform>();
        foreach (var bone in skm.bones)
        {
            foreach (var targetBone in hips)
            {
                if (targetBone.name == bone.name)
                {
                    bones.Add(targetBone);
                    break;
                }
            }
        }
        // Replace the target's part with the new mesh, materials, and bones.
        smr[part].bones = bones.ToArray();
        smr[part].materials = skm.materials;
        smr[part].sharedMesh = skm.sharedMesh;

        SaveDataForPart(part, num, str);
    }

    // Updates the record for the given part in the string array.
    void SaveDataForPart(string part, string num, string[,] str)
    {
        int length = str.GetLength(0);
        for (int i = 0; i < length; i++)
        {
            if (str[i, 0] == part)
            {
                str[i, 1] = num;
            }
        }
    }

    // Initializes the avatar using the default parts specified in avatarStr.
    void InitAvatar()
    {
        int length = avatarStr.GetLength(0);
        for (int i = 0; i < length; i++)
        {
            ChangeMesh(avatarStr[i, 0], avatarStr[i, 1], data, hips, smr, avatarStr);
        }
    }

    // Public method to change a part on the fly.
    // This method is called by our UI (or other game logic) to change a part variant.
    public void OnChangePart(string part, string num)
    {
        ChangeMesh(part, num, data, hips, smr, avatarStr);
    }

    // Allows swapping the entire avatar prefab at runtime.
    public void ChangeAvatar(GameObject newModelPrefab)
    {
        if (newModelPrefab == null)
        {
            Debug.LogError("New avatar prefab is null!");
            return;
        }

        if (target != null)
        {
            Destroy(target);
        }

        modelPrefab = newModelPrefab;
        InstantiateAvatar();
        SaveData(sourceTrans, data, target, smr);
        InitAvatar();
    }

    // --- Public Method to Expose Available Variants for a Category ---
    // This method allows other scripts (like a dynamic UI manager) to query the available parts for a given category.
    public bool GetPartsForCategory(string category, out Dictionary<string, SkinnedMeshRenderer> parts)
    {
        if (data.ContainsKey(category))
        {
            parts = data[category];
            return true;
        }
        parts = null;
        return false;
    }
}