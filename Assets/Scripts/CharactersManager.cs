using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CharactersManager : MonoBehaviour
{
    [SerializeField]
    private GameObject CharacterToSpawn;
    public SpawningCharactersManager[] spawnCharacters;
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

    private string saveFilePath = "/saveGame";

    private void Awake()
    {
        SpawnCharacters();
        SaveButton.onClick.AddListener(SaveCharacterStats);
        LoadButton.onClick.AddListener(LoadCharactersStats);
    }

    void SpawnCharacters()
    {
        for (int i = 0; i < spawnCharacters.Length; i++)
        {
            GameObject currentCharacter = Instantiate(CharacterToSpawn, spawnCharacters[i].spawnPoints, Quaternion.identity);
            var character = spawnCharacters[i];
            currentCharacter.name = character.characterName;
            var controller = currentCharacter.GetComponent<CharacterController>();
            controller.Speed = character.Speed;
            controller.Durability = character.Durability;
            controller.Agility = character.Agility;

            var agent = currentCharacter.GetComponent<NavMeshAgent>();
            agent.speed = spawnCharacters[i].Speed;
            agent.stoppingDistance = 2;


            SpawnedCharacters.Add(currentCharacter);
            if (i is 0) SetLeader(currentCharacter);
            CreateButtons(i);
        }
    }

    void CreateButtons(int i)
    {
        Button button = Instantiate(CharacterButton, ButtonContainer.transform);
        button.GetComponentInChildren<TMP_Text>().text = spawnCharacters[i].characterName;
        int tempValue = i;
        button.onClick.AddListener(delegate { ChangeCharacters(tempValue); });
    }

    void ChangeCharacters(int CharacterID)
    {
        foreach(GameObject character in SpawnedCharacters)
        {
            character.GetComponent<CharacterController>().isLeader = false;
        }

        SetLeader(SpawnedCharacters[CharacterID]);
    }

    private void SetLeader(GameObject leader)
    {
        Leader = leader;
        leader.GetComponent<CharacterController>().isLeader = true;
        CameraController.target = leader.transform;
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



