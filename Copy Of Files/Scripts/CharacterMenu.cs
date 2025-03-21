using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CharacterSelectionMenu : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    public List<GameObject> characterPrefabs = new List<GameObject>();

    void Start()
    {
        LoadCharactersFromFolder();
        GenerateCharacterButtons();
    }

    void LoadCharactersFromFolder()
    {
        GameObject[] loadedPrefabs = Resources.LoadAll<GameObject>("Characters");
        characterPrefabs.AddRange(loadedPrefabs);

        if (characterPrefabs.Count == 0)
        {
            Debug.LogError("No character prefabs found in Resources/Characters!");
        }
    }

    void GenerateCharacterButtons()
    {
        foreach (GameObject characterPrefab in characterPrefabs)
        {
            GameObject newButton = Instantiate(buttonPrefab, buttonContainer);

            Text buttonText = newButton.GetComponentInChildren<Text>();
            if (buttonText != null)
            {
                buttonText.text = characterPrefab.name;
            }
            else
            {
                Debug.LogError($"Button prefab missing a Text component: {newButton.name}");
            }

            newButton.GetComponent<Button>().onClick.AddListener(() => SelectCharacter(characterPrefab));
        }
    }

    public void SelectCharacter(GameObject characterPrefab)
    {
        if (AvatarSys._instance == null)
        {
            Debug.LogError("AvatarSys instance is not available!");
            return;
        }

        AvatarSys._instance.ChangeAvatar(characterPrefab);
    }
}