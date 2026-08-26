using UnityEngine;

[CreateAssetMenu(fileName = "DifficultyTemplate", menuName = "ScriptableObjects/DifficultyTemplate")]
public class DifficultyTemplate : ScriptableObject
{
    [Header("Basic Configurations")]
    public string difficultyName;
    public int solverDifficulty;

    public int roomCount = 2;
    public int minObjects = 1;
    public int maxObjects = 2;
    public int durationUntilObjectIncrease = 2;
    public int minDoors = 2; // probably keep this here - chris
    public int maxDoors = 0; // If less than one, there is no max
    public float roomsPerDoorIncrease = 1;
    
    [Header("Complexity")]
    public int minStatementsBeforeProgress = 1; // not sure
    public int maxCompoundStatements = 0;
    public int durationUntilCompoundIncrease = 0;
}
