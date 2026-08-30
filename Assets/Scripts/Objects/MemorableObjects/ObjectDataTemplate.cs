using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectDataTemplate", menuName = "ScriptableObjects/ObjectDataTemplate")]
public class ObjectDataTemplate : ScriptableObject
{
    [SerializeField] public GameObject objectPrefab;
    [SerializeField] public float chance;
    [SerializeField] public string roomTemplate;
    [SerializeField] public List<int> rowsTaken; // determines which rows are taken up by this object
    [SerializeField] public int rowSource; // determines which point of origin in global world is the object placed at
    [SerializeField] public float invalidationRange;
    [SerializeField] public List<ObjectPropertyData> objectPropertyDatas;

    public GameObject GetObjectPrefab()
    {
        return objectPrefab;
    }
    public float GetChance()
    {
        return chance;
    }

    public List<ObjectPropertyData> GetObjectPropertyDatas()
    {
        return objectPropertyDatas;
    }
    public string GetRoomTemplate()
    {
        return roomTemplate;
    }
    public List<int> GetRowsTaken()
    {
        return rowsTaken;
    }
    public int GetRowSource()
    {
        return rowSource;
    }
    public float GetInvalidationRange()
    {
        return invalidationRange;
    }
    
}

[System.Serializable]
public class ObjectPropertyData
{
    public string propertyName;
    public string template;
    public List<string> values;
    public List<Sprite> sprites;
}
