using System;
using Unity.VisualScripting;
using UnityEngine;

public class RoomGenerator : MonoBehaviour
{
    // Singleton -------------------------------
    private static RoomGenerator _instance;

    public static RoomGenerator Instance { get { return _instance; } }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple RoomGenerator scripts. Bad!");
        }
        else
        {
            _instance = this;
        }
    }
    // Singleton -------------------------------

    [Header("References")]
    [SerializeField] private GameObject doorPrefab;

    [Header("Parameters")]
    [SerializeField] private int roomCount;
    [SerializeField] private float roomDistance;
    [SerializeField] private float doorDistance;
    
    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        for (int i = 0; i < roomCount; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                PlaceDoor(new Vector3((doorDistance * j) - doorDistance, 0, (i * roomDistance) + roomDistance), j);
            }
        }
    }

    private void PlaceDoor(Vector3 position, int doorNumber)
    {
        GameObject door = Instantiate(doorPrefab, position, Quaternion.identity, transform);
        Door doorScript = door.GetComponent<Door>();
        if (doorScript == null)
        {
            throw new Exception("Door script not found on door prefab!");
        }

        doorScript.SetNumber(doorNumber);
    }
}
