using UnityEngine;
using LogicSolver;
public class Clock : MonoBehaviour
{
    KnownFact ClockTime = new KnownFact();
    KnownFact ClockColor = new KnownFact();

    // Constructor
    public Clock()
    {
        // initialize the plant's potential possible values
        ClockTime.possibleValues = new string[] {"two", "four", "six", "eight"};
        ClockTime.template = "the clock in the last room read a {0} o'clock";

        ClockColor.possibleValues = new string[] {"blue", "red", "black", "white"};
        ClockColor.template = "the clock in the last room was a {0} color";
        
    }

    public void GenerateRandomTime(int randNum)
    {
        ClockTime.actualValue = ClockTime.possibleValues[randNum];

    }

    public void GenerateRandomColor(int randNum)
    {
        ClockColor.actualValue = ClockColor.possibleValues[randNum];
    }
}
