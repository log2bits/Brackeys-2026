using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ObjectDataTemplate", menuName = "ScriptableObjects/ObjectDataTemplate")]
public class ObjectDataTemplate : ScriptableObject
{
    [SerializeField] private GameObject objectPrefab;
    [SerializeField] private string roomTemplate;
    [SerializeField] private List<int> rowsTaken; // determines which rows are taken up by this object
    [SerializeField] private int rowSource; // determines which point of origin in global world is the object placed at
    [SerializeField] private float invalidationRange;
    [SerializeField] private List<ObjectPropertyData> objectPropertyDatas;

    public GameObject GetObjectPrefab()
    {
        return objectPrefab;
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
