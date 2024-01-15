using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CharactersManager : MonoBehaviour
{
    [SerializeField]
    private GameObject CharacterToSpawn;

    private List<SpawningCharactersManager> spawnCharacters = new();
    [SerializeField]
    private Button CharacterButton;
    [SerializeField]
    private GameObject ButtonContainer;
    private List <GameObject> SpawnedCharacters = new();
    public static GameObject Leader;

    [SerializeField]
    private Button LoadButton;
    [SerializeField]
    private Button SaveButton;

    private GameObject LeaderPointer;

    private string saveFilePath = "/saveGame";

    private void Awake()
    {
        LeaderPointer = GameObject.Find("SelectedCharacter");
        StartCoroutine(GetScriptableObjects());
        SaveButton.onClick.AddListener(SaveCharacterStats);
        LoadButton.onClick.AddListener(LoadCharactersStats);
    }

    void SpawnCharacters()
    {
        for (int i = 0; i < spawnCharacters.Count; i++)
        {
            GameObject currentCharacter = Instantiate(CharacterToSpawn, spawnCharacters[i].spawnPoints, Quaternion.identity);
            var model = Instantiate(spawnCharacters[i].Model, currentCharacter.transform.position, Quaternion.identity);
            model.transform.parent = currentCharacter.transform;

            var character = spawnCharacters[i];
            currentCharacter.name = character.characterName;

            var controller = currentCharacter.GetComponent<CharacterController>();
            controller.Speed = character.Speed;
            controller.Durability = character.Durability;
            controller.Agility = character.Agility;
            controller._animator = model.GetComponent<Animator>();

            SpawnedCharacters.Add(currentCharacter);
            if (i is 0) SetLeader(currentCharacter);
            CreateButtons(i, controller);
        }
    }

    void CreateButtons(int i, CharacterController controller)
    {
        Button button = Instantiate(CharacterButton, ButtonContainer.transform);
        button.GetComponentInChildren<TMP_Text>().text = spawnCharacters[i].characterName;
        int tempValue = i;
        button.onClick.AddListener(delegate { ChangeCharacters(tempValue); });
        controller.staminaBar = button.transform.Find("Slider").GetComponent<Slider>();
    }

    void ChangeCharacters(int CharacterID)
    {
        foreach(GameObject character in SpawnedCharacters)
        {
            character.GetComponent<CharacterController>().isLeader = false;
        }
        var leader = SpawnedCharacters[CharacterID];
        SetLeader(leader);
    }

    private void SetLeader(GameObject leader)
    {
        Leader = leader;
        leader.GetComponent<CharacterController>().isLeader = true;
        CameraController.target = leader.transform;

        LeaderPointer.transform.parent = leader.transform;
        LeaderPointer.transform.position = leader.transform.position;
    }

    void LoadCharactersStats()
    {
        List<SaveData> stats = SaveGameSystem.LoadPlayerData(saveFilePath);
        for(int i = 0; i < SpawnedCharacters.Count; i++)
        {
            var character = SpawnedCharacters[i].GetComponent<CharacterController>();
            character.LoadParameters(stats[i].Speed, stats[i].Agility, stats[i].Durability);
        }
    }
    
    public IEnumerator GetScriptableObjects()
    {
        while (!LoadCharacters.CharactersLoaded)
            yield return null;

        List<SpawningCharactersManager> list = LoadCharacters.characterList;

        foreach (SpawningCharactersManager character in list)
        {
            spawnCharacters.Add(character);
        }
        SpawnCharacters();
    }

    void SaveCharacterStats()
    {
        List<SaveData> save = new();
        foreach(GameObject obj in SpawnedCharacters)
        {
            SaveData stats = new();
            var character = obj.GetComponent<CharacterController>();
            stats.Speed = character.Speed;
            stats.Durability = character.Durability;
            stats.Agility = character.Agility;
            save.Add(stats);            
        }
        
        SaveGameSystem.SavePlayerData(save, saveFilePath);
    }
}



