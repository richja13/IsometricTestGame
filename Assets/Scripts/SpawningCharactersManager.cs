using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character")]
public class SpawningCharactersManager : ScriptableObject, ICharacter
{
    public string characterName;
    public Vector3 spawnPoints;
    public float Speed { get; set; }
    public float Agility { get; set; }
    public float Durability { get; set; }

    private void OnEnable()
    {
        Durability = GenerateRandomStats.GenerateRandomValue(1, 4);
        Speed = GenerateRandomStats.GenerateRandomValue(2, 6);
        Agility = GenerateRandomStats.GenerateRandomValue(5, 12);
    }
}
