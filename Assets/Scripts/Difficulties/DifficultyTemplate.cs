using UnityEngine;

[CreateAssetMenu(fileName = "ItemType", menuName = "ScriptableObjects/ItemType")]
public class DifficultyTemplate : ScriptableObject
{
    [Header("Basic Configurations")]
    public string difficultyName;

    public int roomCount; // if null, continues forever ?
    public int minObjects = 1;
    public int maxObjects = 2;
    public int durationUntilObjectIncrease = 2;
    public int minDoors = 2; // probably keep this here - chris
    public int maxDoors; // if null, it continues forever
    public int durationUntilDoorIncrease = 1;
    
    [Header("Complexity")]
    public int minStatementsBeforeProgress = 1; // not sure
    public int maxCompoundStatements = 0;
    public int durationUntilCompoundIncrease = 0;
}
