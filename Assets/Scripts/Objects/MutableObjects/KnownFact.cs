using UnityEngine;

public sealed class KnownFact
{
    // Everything it could have been, for example red, blue, yellow
    public string[] possibleValues;

    // How a guard phrases it, with {0} where the value goes
    public string template;

    public string actualValue;
    public string Say(string value) 
    { 
        //template.Replace("{1}", room.ToString());
        return template.Replace("{0}", value); 
    }
    public bool IsTrue(string value) { return value == actualValue; }
}