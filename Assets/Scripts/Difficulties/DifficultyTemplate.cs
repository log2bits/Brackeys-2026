using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyTemplate", menuName = "ScriptableObjects/DifficultyTemplate")]
public class DifficultyTemplate : ScriptableObject
{
    [Header("Basic Configurations")]
    public string difficultyName;
    public string difficultyDescription;

    public int solverDifficulty;
    public int roomCount = 2;
    public float objectMultiplier = 1.0f;
    public int minObjects = 1;
    public int maxObjects = 2;
    public int durationUntilObjectIncrease = 2;
    public int minDoors = 2; // probably keep this here - chris
    public int maxDoors = 0; // If less than one, there is no max
    public float roomsPerDoorIncrease = 1;

    [Header("Complexity")]
    public int detailMentions = 1; // how many things the player has to remember
}