using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarCustomizer : MonoBehaviour
{
    [Header("Customization Settings")]
    [Tooltip("Hidden model repository containing all available parts (must have a 'Parts' folder).")]
    [SerializeField] private GameObject modelPrefab;

    // Data structures for storing parts info.
    private Transform sourceTrans;  // from the hidden repository
    private Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data = new Dictionary<string, Dictionary<string, SkinnedMeshRenderer>>();
    private Transform[] hips;
    private Dictionary<string, SkinnedMeshRenderer> smr = new Dictionary<string, SkinnedMeshRenderer>();

    // Default parts selection: { category, defaultVariant }
    // Ensure these match your repository's naming exactly.
    private string[,] avatarStr = new string[,] {
        { "Hair", "1" },
        { "Top", "1" },
        { "Bottom", "1" },
        { "Shoes", "1" },
        { "Face", "A1" }
    };

    public GameObject CurrentAvatar => gameObject;
    public Animator CurrentAnimator { get; private set; }

    void Awake()
    {
        CurrentAnimator = GetComponent<Animator>();
        if (CurrentAnimator == null)
        {
            Debug.LogError("No Animator component found on the avatar!");
        }
    }

    void Start()
    {
        // Instantiate the hidden model repository as a child.
        if (modelPrefab == null)
        {
            Debug.LogError("Please assign the modelPrefab repository for customization parts!");
            return;
        }
        GameObject sourceGO = Instantiate(modelPrefab, transform.position, Quaternion.identity, transform);
        sourceTrans = sourceGO.transform;
        sourceGO.SetActive(false); // hide repository

        // Get bones from the avatar for remapping.
        hips = GetComponentsInChildren<Transform>();

        // Collect part data.
        SaveData(sourceTrans, data, gameObject, smr);

        // Optionally, you might want to clear any pre-existing parts.
        // (If your target already has parts that you want to replace.)
        // Here we leave them in place so that ChangeMesh will override them.

        // Apply defaults.
        InitAvatar();
    }

    // Scans the hidden repository for parts and sets up the data dictionaries.
    void SaveData(Transform sourceTrans, Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data, GameObject target, Dictionary<string, SkinnedMeshRenderer> smr)
    {
        data.Clear();
        smr.Clear();

        if (sourceTrans == null)
            return;

        Transform partsParent = sourceTrans.Find("Parts");
        if (partsParent == null)
        {
            Debug.LogError("No 'Parts' folder found in source model!");
            return;
        }

        foreach (Transform category in partsParent)
        {
            Debug.Log("Found category in repository: " + category.name);
            if (!data.ContainsKey(category.name))
                data.Add(category.name, new Dictionary<string, SkinnedMeshRenderer>());

            // Find (or create) the corresponding category folder in the target.
            Transform targetPartsParent = target.transform.Find("Parts");
            if (targetPartsParent == null)
            {
                Debug.LogError("No 'Parts' folder found in target model! Please create one in your avatar prefab.");
                continue;
            }
            Transform targetCategory = targetPartsParent.Find(category.name);
            if (targetCategory == null)
            {
                GameObject newCategory = new GameObject(category.name);
                newCategory.transform.parent = targetPartsParent;
                targetCategory = newCategory.transform;
            }

            foreach (Transform part in category)
            {
                SkinnedMeshRenderer partSMR = part.GetComponent<SkinnedMeshRenderer>();
                if (partSMR != null)
                {
                    // Expecting names like "Hair_1", "Hair_2", etc.
                    data[category.name].Add(part.name, partSMR);
                    Debug.Log("Collected part: " + part.name + " under category: " + category.name);
                }
            }

            if (!smr.ContainsKey(category.name))
            {
                GameObject partGo = new GameObject(category.name);
                partGo.transform.parent = targetCategory;
                smr.Add(category.name, partGo.AddComponent<SkinnedMeshRenderer>());
            }
        }
    }

    // Changes the mesh for the given category.
    void ChangeMesh(string part, string num, Dictionary<string, Dictionary<string, SkinnedMeshRenderer>> data, Transform[] hips, Dictionary<string, SkinnedMeshRenderer> smr, string[,] str)
    {
        string fullPartName = part + "_" + num;

        if (!data.ContainsKey(part) || !data[part].ContainsKey(fullPartName))
        {
            Debug.LogError("No part found with key " + fullPartName);
            return;
        }

        SkinnedMeshRenderer skm = data[part][fullPartName];

        // Clear any previously applied mesh on this category.
        if (smr.ContainsKey(part))
        {
            smr[part].sharedMesh = null;
            smr[part].materials = new Material[0];
        }

        // Remap bones.
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
        smr[part].bones = bones.ToArray();
        smr[part].materials = skm.materials;
        smr[part].sharedMesh = skm.sharedMesh;

        SaveDataForPart(part, num, str);
    }

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

    // Automatically applies the default parts.
    void InitAvatar()
    {
        int length = avatarStr.GetLength(0);
        for (int i = 0; i < length; i++)
        {
            ChangeMesh(avatarStr[i, 0], avatarStr[i, 1], data, hips, smr, avatarStr);
        }
    }

    // Called by the UI to change a part.
    public void OnChangePart(string part, string num)
    {
        ChangeMesh(part, num, data, hips, smr, avatarStr);
        Debug.Log($"Changed {part} to variant {num}");
    }

    // Exposes available variants for a category.
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