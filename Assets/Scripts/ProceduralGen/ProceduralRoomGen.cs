using UnityEngine;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
//using System.Numerics;

public class ProceduralRoomGen : MonoBehaviour
{
    // Singleton -------------------------------
    private static ProceduralRoomGen _instance;

    public static ProceduralRoomGen Instance { get { return _instance; } }

    System.Random randomNumberGenerator;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            Debug.LogWarning("Multiple ProceduralRoomGen scripts. Bad!");
        }
        else
        {
            _instance = this;
        }


        doorSpriteRenderer = doorFullPrefab.transform.GetChild(0).GetComponent<SpriteRenderer>();
        doorSize = doorSpriteRenderer.sprite.bounds.size.x * Mathf.Abs(doorSpriteRenderer.transform.localScale.x);
        
        int seed = (int)(DateTime.UtcNow.Ticks % int.MaxValue);
        Debug.Log($"Generated Seed: {seed}");

        
        randomNumberGenerator = new System.Random(seed);
    }


    [Header("References")]
    [SerializeField] private GameObject doorFullPrefab;

    [Header("Parameters")]
    [SerializeField] private int roomCount = 3;
    [SerializeField] private float roomDistance = 20;
    //[SerializeField] private float doorDistance = 15; // accounts for doorframes
    [SerializeField] private float sideWallsDistance = 15;

    [Header("Objects")]
    [SerializeField] private int objects = 3;

    // Initialized variables
    private float doorSize;
    private SpriteRenderer doorSpriteRenderer;

    // Constants
    const int INITIAL_DOOR_COUNT = 2;

    // Saves current doors in the room
    private List<DoorData> currentDoors = new List<DoorData>();

    struct DoorData
    {
        public GameObject door;
        public bool safe;

        public DoorData(GameObject givenDoor, bool isSafe)
        {
            door = givenDoor;
            safe = isSafe;
        }
    }

    // StartGenerationProcess
    public void GenerateProcess()
    {
        GenerateNextRoom(transform.position, 2);

        System.Random rnd = new System.Random();
        
        for (int room = 1; room < roomCount; room++)
        {
            int randomInt = rnd.Next(0, room + 1);
            Debug.Log($"Random int: {randomInt}");
            Debug.Log($"Current Doors: {currentDoors.Count}");
            Vector3 nextDoorPosition = currentDoors[randomInt].door.transform.position;
            nextDoorPosition.z += roomDistance;
            GenerateNextRoom(nextDoorPosition, room + INITIAL_DOOR_COUNT);
        }

        // Clear all doors
        currentDoors.Clear();
    }

    // 
    private void GenerateNextRoom(Vector3 centralPosition, int doorCount)
    {
        GenerateDoors(centralPosition, doorCount);
    }

    public void GenerateDoors(Vector3 centralPosition, int doorCount)
    {
        if (doorFullPrefab == null) throw new Exception("ProceduralRoomGen: DoorPrefab is null");
        // safely clear the currnet doors which are stored
        currentDoors.Clear();

        // Collect door size, offset initially, and width

        float offsetDoorSide = (doorCount % 2 == 0) ? doorSize / 2f : 0;
        float width = doorSize * doorCount;
        Debug.Log($"Object Size (W): {doorSize}");
        Debug.Log($"Door Count: {doorCount}");
        
        // Iterate through doors
        for (int currDoor = 0; currDoor < doorCount; currDoor++)
        {
            // finds the position of the x coordinate of the new door, offsetting for initial off, and for door frames
            
            Vector3 newPosition = new Vector3(centralPosition.x + (((int)(currDoor - doorCount / 2) * (doorSize)) + offsetDoorSide), centralPosition.y, centralPosition.z);
            GameObject door = Instantiate(doorFullPrefab, newPosition, Quaternion.identity, transform);
            door.transform.GetChild(1).GetComponent<Door>().SetNumber(currDoor);

            // save the door
            currentDoors.Add(new DoorData(door, false));
        }
    }
}
