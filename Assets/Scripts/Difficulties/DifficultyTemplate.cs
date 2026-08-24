using UnityEngine;

public class DifficultyTemplate
{
    public string name;

    public int rooms; // if null, continues forever ?
    public int durationUntilObjectIncrease = 2;
    public int minDoors = 2; // probably keep this here - chris
    public int maxDoors; // if null, it continues forever
    public int durationUntilDoorIncrease = 1;
    
    public int minStatementsBeforeProgress = 1; // not sure
    public int maxCompoundStatements = 0;
    public int durationUntilCompoundIncrease = 0;
}
